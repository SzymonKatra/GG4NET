using System;
using System.Text;
using System.Security.Cryptography;

namespace GG4NET
{
    /// <summary>
    /// Metody pomocnicze.
    /// </summary>
    public static class Utils
    {
        //SOURCE: http://www.sanity-free.com/12/crc32_implementation_in_csharp.html
        private static class Crc32
        {
            private static uint[] table;

            public static uint ComputeChecksum(byte[] bytes)
            {
                uint crc = 0xffffffff;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                    crc = (uint)((crc >> 8) ^ table[index]);
                }
                return ~crc;
            }
            public static byte[] ComputeChecksumBytes(byte[] bytes)
            {
                return BitConverter.GetBytes(ComputeChecksum(bytes));
            }

            static Crc32()
            {
                uint poly = 0xedb88320;
                table = new uint[256];
                uint temp = 0;
                for (uint i = 0; i < table.Length; ++i)
                {
                    temp = i;
                    for (int j = 8; j > 0; --j)
                    {
                        if ((temp & 1) == 1)
                        {
                            temp = (uint)((temp >> 1) ^ poly);
                        }
                        else
                        {
                            temp >>= 1;
                        }
                    }
                    table[i] = temp;
                }
            }
        }

        /// <summary>
        /// Oblicza sumę kontrolną CRC32 z podanych danych.
        /// </summary>
        /// <param name="data">Dane.</param>
        /// <returns>Suma kontrolna CRC32.</returns>
        public static uint ComputeCrc32(byte[] data)
        {
            return Crc32.ComputeChecksum(data);
        }
        /// <summary>
        /// Oblicz hash obrazka.
        /// </summary>
        /// <param name="crc32">Suma kontrolna CRC32.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        /// <returns>Hash obrazka.</returns>
        public static string ComputeHash(uint crc32, uint length)
        {
            return crc32.ToString("X8") + length.ToString("X8");
        }
        /// <summary>
        /// Parsuj hash obrazka.
        /// </summary>
        /// <param name="hash">Hash.</param>
        /// <param name="crc32">Suma kontrolna CRC32.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        public static void ParseImageHash(string hash, out uint crc32, out uint length)
        {
            crc32 = 0;
            length = 0;
            try
            {
                if (hash.Length != 16) throw new InvalidOperationException("Bad hash length");

                crc32 = Convert.ToUInt32(hash.Remove(8), 16);
                length = Convert.ToUInt32(hash.Remove(0, 8), 16);
            }
            catch { throw new InvalidOperationException("Bad hash"); }
        }

        internal static byte[] ComputeSHA1(string password, uint seed)
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

        internal static uint ToInternalStatus(Status status, bool description)
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
        internal static Status ToPublicStatus(uint status, out bool description)
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

        internal static uint ToInternalContactType(ContactType type)
        {
            switch (type)
            {
                case ContactType.Offline: return Container.GG_USER_OFFLINE;
                case ContactType.Normal: return Container.GG_USER_NORMAL;
                case ContactType.Blocked: return Container.GG_USER_BLOCKED;
                default: return 0;
            }
        }
        internal static ContactType ToPublicContactType(uint type)
        {
            switch (type)
            {
                case Container.GG_USER_OFFLINE: return ContactType.Offline;
                case Container.GG_USER_NORMAL: return ContactType.Normal;
                case Container.GG_USER_BLOCKED: return ContactType.Blocked;
                default: return ContactType.None;
            }
        }

        internal static ushort ToInternalTypingNotify(TypingNotifyType type)
        {
            switch (type)
            {
                case TypingNotifyType.Start: return Container.GG_TYPING_NOTIFY_TYPE_START;
                case TypingNotifyType.Stop: return Container.GG_TYPING_NOTIFY_TYPE_STOP;
                default: return 0;
            }
        }
        internal static TypingNotifyType ToPublicTypingNotify(ushort type)
        {
            switch (type)
            {
                case Container.GG_TYPING_NOTIFY_TYPE_START: return TypingNotifyType.Start;
                case Container.GG_TYPING_NOTIFY_TYPE_STOP: return TypingNotifyType.Stop;
                default: return TypingNotifyType.None;
            }
        }

        internal static uint ToInternalGender(Gender gender, bool inverted = false)
        {
            switch (gender)
            {
                case Gender.Female: return (inverted ? Container.GG_PUBDIR50_GENDER_SET_FEMALE : Container.GG_PUBDIR50_GENDER_FEMALE);
                case Gender.Male: return (inverted ? Container.GG_PUBDIR50_GENDER_SET_MALE : Container.GG_PUBDIR50_GENDER_MALE);
                default: return 0;
            }
        }
        internal static Gender ToPublicGender(uint gender, bool inverted = false)
        {
            switch (gender)
            {
                case Container.GG_PUBDIR50_GENDER_FEMALE: return (inverted ? Gender.Male : Gender.Female);
                case Container.GG_PUBDIR50_GENDER_MALE: return (inverted ? Gender.Female : Gender.Male);
                default: return Gender.None;
            }
        }

        internal static byte ToInternalContactListFormat(ContactListType type)
        {
            switch (type)
            {
                case ContactListType.CSV: return Container.GG_USERLIST100_FORMAT_TYPE_GG70;
                case ContactListType.XML: return Container.GG_USERLIST100_FORMAT_TYPE_GG100;
                default: return Container.GG_USERLIST100_FORMAT_TYPE_NONE;
            }
        }
        internal static ContactListType ToPublicContactListFormat(byte type)
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
