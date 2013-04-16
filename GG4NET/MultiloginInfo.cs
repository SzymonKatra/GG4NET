using System;
using System.Net;

namespace GG4NET
{
    /// <summary>
    /// Informacje na temat multilogowania.
    /// </summary>
    public class MultiloginInfo
    {
        /// <summary>
        /// IP klienta który uczestniczy w sesji.
        /// </summary>
        public IPAddress IP;
        /// <summary>
        /// Data zalogowania.
        /// </summary>
        public DateTime LogonTime;
        internal uint Flags;
        internal uint Features;
        /// <summary>
        /// Indetyfikator połączenia.
        /// </summary>
        public ulong ConnectionId;
        /// <summary>
        /// Nazwa klienta.
        /// </summary>
        public string ClientName;
    }
}
