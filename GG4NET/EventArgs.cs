using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG4NET
{
    public class MessageEventArgs : EventArgs
    {
        private uint _uin;
        private DateTime _time;
        private string _message;
        private string _htmlMessage;
        private byte[] _attributes;

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
        public byte[] Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        public MessageEventArgs()
            : this(0, DateTime.MinValue, string.Empty, string.Empty, null)
        {
        }
        public MessageEventArgs(uint uin, DateTime time, string message)
            : this(uin, time, message, message, null)
        {
        }
        public MessageEventArgs(uint uin, DateTime time, string message, string htmlMessage)
            : this(uin, time, message, htmlMessage, null)
        {
        }
        public MessageEventArgs(uint uin, DateTime time, string message, string htmlMessage, byte[] attributes)
        {
            _uin = uin;
            _time = time;
            _message = message;
            _htmlMessage = htmlMessage;
            _attributes = attributes;
        }
    }

    public class MultiloginEventArgs : EventArgs
    {
        private MultiloginInfo _info;

        public MultiloginInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }

        public MultiloginEventArgs(MultiloginInfo info)
        {
            _info = info;
        }
    }

    public class PublicDirectoryReplyEventArgs : EventArgs
    {
        private PublicDirectoryReply[] _reply;
        private uint _nextStart = 0;

        public PublicDirectoryReply[] Reply
        {
            get { return _reply; }
            set { _reply = value; }
        }
        public uint NextStart
        {
            get { return _nextStart; }
            set { _nextStart = value; }
        }

        public PublicDirectoryReplyEventArgs()
        {
        }
        public PublicDirectoryReplyEventArgs(PublicDirectoryReply[] reply, uint nextStart = 0)
        {
            _reply = reply;
            _nextStart = nextStart;
        }
    }

    public class StatusEventArgs : EventArgs
    {
        private ContactInfo _contact;

        public ContactInfo Contact
        {
            get { return _contact; }
            set { _contact = value; }
        }

        public StatusEventArgs()
            : this(new ContactInfo())
        {
        }
        public StatusEventArgs(ContactInfo contact)
        {
            _contact = contact;
        }
    }

    public class TypingNotifyEventArgs : EventArgs
    {
        private uint _uin = 0;
        private TypingNotifyType _type = TypingNotifyType.None;
        private ushort _length = 0;

        public uint Uin
        {
            get { return _uin; }
            set { _uin = value; }
        }
        public TypingNotifyType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public ushort Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public TypingNotifyEventArgs()
            : this(0, TypingNotifyType.None, 0)
        {
        }
        public TypingNotifyEventArgs(uint uin)
            : this(uin, TypingNotifyType.None, 0)
        {
        }
        public TypingNotifyEventArgs(uint uin, ushort length)
            : this(uin, (length >= 1 ? TypingNotifyType.Start : TypingNotifyType.Stop), length)
        {
        }
        public TypingNotifyEventArgs(uint uin, TypingNotifyType type)
            : this(uin, type, 0)
        {
        }
        public TypingNotifyEventArgs(uint uin, TypingNotifyType type, ushort length)
        {
            _uin = uin;
            _type = type;
            _length = length;
        }
    }
}
