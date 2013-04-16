using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG4NET
{
    public enum GGPort
    {
        None = 0,
        P8074 = 8074,
        P443 = 443,
    }

    public enum Status
    {
        None = 0,
        NotAvailable,
        Available,
        Busy,
        Invisible,
        DoNotDisturb,
        FreeForCall,
        Blocked,
    }

    public enum ContactType
    {
        None = 0,
        Offline,
        Normal,
        Blocked,
    }

    public enum TypingNotifyType
    {
        None = 0,
        Start,
        Stop,
    }

    public enum Gender
    {
        None,
        Female,
        Male,
    }
}
