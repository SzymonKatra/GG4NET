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
        None,
        NotAvailable,
        Available,
        Busy,
        Invisible,
        DoNotDisturb,
        FreeForCall,
        Blocked,
    }
}
