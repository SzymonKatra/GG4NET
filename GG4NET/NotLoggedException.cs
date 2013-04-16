using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GG4NET
{
    public class NotLoggedException : Exception
    {
        public NotLoggedException(string message)
            : base(message)
        {
        }
    }
}
