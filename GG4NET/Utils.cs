using System;
using System.Text;
using System.Security.Cryptography;

namespace GG4NET
{
    internal static class Utils
    {
        public static byte[] CalculateSHA1Hash(string password, uint seed)
        {
            SHA1 sha = SHA1.Create();
            byte[] hash = new byte[64];
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] seedBytes = BitConverter.GetBytes(seed);
            byte[] toEncode = new byte[passwordBytes.Length + seedBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, toEncode, 0, passwordBytes.Length);
            Buffer.BlockCopy(seedBytes, 0, toEncode, passwordBytes.Length, seedBytes.Length);
            sha.ComputeHash(toEncode).CopyTo(hash, 0);
            return hash;
        }
        public static uint ToInternalStatus(Status status, bool description)
        {
            switch (status)
            {
                case Status.Available: if (description) return Container.GG_STATUS_AVAIL_DESCR; else return Container.GG_STATUS_AVAIL;
                case Status.Blocked: return Container.GG_STATUS_BLOCKED;
                case Status.Busy: if (description) return Container.GG_STATUS_BUSY_DESCR; else return Container.GG_STATUS_BUSY;
                case Status.DoNotDisturb: if (description) return Container.GG_STATUS_DND_DESCR; else return Container.GG_STATUS_DND;
                case Status.FreeForCall: if (description) return Container.GG_STATUS_FFC_DESCR; else return Container.GG_STATUS_FFC;
                case Status.Invisible: if (description) return Container.GG_STATUS_INVISIBLE_DESCR; else return Container.GG_STATUS_INVISIBLE;
                case Status.NotAvailable: if (description) return Container.GG_STATUS_NOT_AVAIL_DESCR; else return Container.GG_STATUS_NOT_AVAIL;
            }
            return 0x0;
        }
        public static Status ToPublicStatus(uint status, out bool description)
        {
            description = false;
            switch (status)
            {
                case Container.GG_STATUS_AVAIL: return Status.Available;
                case Container.GG_STATUS_AVAIL_DESCR: description = true; return Status.Available;
                case Container.GG_STATUS_BLOCKED: return Status.Blocked;
                case Container.GG_STATUS_BUSY: return Status.Busy;
                case Container.GG_STATUS_BUSY_DESCR: description = true; return Status.Busy;
                case Container.GG_STATUS_DND: return Status.DoNotDisturb;
                case Container.GG_STATUS_DND_DESCR: description = true; return Status.DoNotDisturb;
                case Container.GG_STATUS_FFC: return Status.FreeForCall;
                case Container.GG_STATUS_FFC_DESCR: description = true; return Status.FreeForCall;
                case Container.GG_STATUS_INVISIBLE: return Status.Invisible;
                case Container.GG_STATUS_INVISIBLE_DESCR: description = true; return Status.Invisible;
                case Container.GG_STATUS_NOT_AVAIL: return Status.NotAvailable;
                case Container.GG_STATUS_NOT_AVAIL_DESCR: description = true; return Status.NotAvailable;
            }
            return Status.None;
        }
    }
}
