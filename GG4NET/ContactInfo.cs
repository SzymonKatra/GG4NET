using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GG4NET
{
    public struct ContactInfo
    {
        public uint Uin;
        public ContactType Type;
        public Status Status { get; internal set; }
        public string Description { get; internal set; }
        public uint MaxImageSize { get; internal set; }
        internal uint Features;
        internal uint Flags;
    }
}
