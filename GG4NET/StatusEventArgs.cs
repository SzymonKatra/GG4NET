using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GG4NET
{
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
}
