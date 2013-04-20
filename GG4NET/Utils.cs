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

        public static uint ToInternalContactType(ContactType type)
        {
            switch (type)
            {
                case ContactType.Offline: return Container.GG_USER_OFFLINE;
                case ContactType.Normal: return Container.GG_USER_NORMAL;
                case ContactType.Blocked: return Container.GG_USER_BLOCKED;
                default: return 0;
            }
        }
        public static ContactType ToPublicContactType(uint type)
        {
            switch (type)
            {
                case Container.GG_USER_OFFLINE: return ContactType.Offline;
                case Container.GG_USER_NORMAL: return ContactType.Normal;
                case Container.GG_USER_BLOCKED: return ContactType.Blocked;
                default: return ContactType.None;
            }
        }

        public static ushort ToInternalTypingNotify(TypingNotifyType type)
        {
            switch (type)
            {
                case TypingNotifyType.Start: return Container.GG_TYPING_NOTIFY_TYPE_START;
                case TypingNotifyType.Stop: return Container.GG_TYPING_NOTIFY_TYPE_STOP;
                default: return 0;
            }
        }
        public static TypingNotifyType ToPublicTypingNotify(ushort type)
        {
            switch (type)
            {
                case Container.GG_TYPING_NOTIFY_TYPE_START: return TypingNotifyType.Start;
                case Container.GG_TYPING_NOTIFY_TYPE_STOP: return TypingNotifyType.Stop;
                default: return TypingNotifyType.None;
            }
        }

        public static uint ToInternalGender(Gender gender, bool inverted = false)
        {
            switch (gender)
            {
                case Gender.Female: return (inverted ? Container.GG_PUBDIR50_GENDER_SET_FEMALE : Container.GG_PUBDIR50_GENDER_FEMALE);
                case Gender.Male: return (inverted ? Container.GG_PUBDIR50_GENDER_SET_MALE : Container.GG_PUBDIR50_GENDER_MALE);
                default: return 0;
            }
        }
        public static Gender ToPublicGender(uint gender, bool inverted = false)
        {
            switch (gender)
            {
                case Container.GG_PUBDIR50_GENDER_FEMALE: return (inverted ? Gender.Male : Gender.Female);
                case Container.GG_PUBDIR50_GENDER_MALE: return (inverted ? Gender.Female : Gender.Male);
                default: return Gender.None;
            }
        }

        public static byte ToInternalContactListFormat(ContactListType type)
        {
            switch (type)
            {
                case ContactListType.CSV: return Container.GG_USERLIST100_FORMAT_TYPE_GG70;
                case ContactListType.XML: return Container.GG_USERLIST100_FORMAT_TYPE_GG100;
                default: return Container.GG_USERLIST100_FORMAT_TYPE_NONE;
            }
        }
        public static ContactListType ToPublicContactListFormat(byte type)
        {
            switch (type)
            {
                case Container.GG_USERLIST100_FORMAT_TYPE_GG70: return ContactListType.CSV;
                case Container.GG_USERLIST100_FORMAT_TYPE_GG100: return ContactListType.XML;
                default: return ContactListType.None;
            }
        }
    }
}
