using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG4NET
{
    public class MessageEventArgs : EventArgs
    {
        private uint _sender;
        private DateTime _time;
        private string _message;
        private string _htmlMessage;

        public uint Sender
        {
            get
            {
                return _sender;
            }
            set
            {
                _sender = value;
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

        public MessageEventArgs(uint sender, DateTime time, string message)
            : this(sender, time, message, message)
        {
        }
        public MessageEventArgs(uint sender, DateTime time, string message, string htmlMessage)
        {
            _sender = sender;
            _time = time;
            _message = message;
            _htmlMessage = htmlMessage;
        }
    }
}
