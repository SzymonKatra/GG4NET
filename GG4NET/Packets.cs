using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GG4NET
{
    internal static class Packets
    {
        public static void ReadWelcome(byte[] data, out uint seed)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                seed = reader.ReadUInt32(); //seed
            }
        }
        public static void ReadReceiveMessage(byte[] data, out uint sender, out uint seq, out DateTime time, out string plainMessage, out string htmlMessage, out byte[] attributes)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                sender = reader.ReadUInt32(); //gg num
                seq = reader.ReadUInt32(); //sequence number = time from 1.1.1970
                time = new DateTime(1970, 1, 1);
                time.AddSeconds(reader.ReadUInt32());
                reader.ReadUInt32(); // message class
                uint plain_offset = reader.ReadUInt32(); //plain offset
                uint attrib_offset = reader.ReadUInt32(); //attributes offset
                htmlMessage = Encoding.UTF8.GetString(reader.ReadBytes((int)(plain_offset - 24))); //read html message
                plainMessage = Encoding.ASCII.GetString(reader.ReadBytes((int)(attrib_offset - plain_offset))); //read plain message
                attributes = reader.ReadBytes((int)(data.Length - reader.BaseStream.Position)); //attributes
            }
        }
        public static void ReadNotifyReply(byte[] data, out List<ContactInfo> contacts)
        {
            contacts = new List<ContactInfo>();
            using (PacketReader reader = new PacketReader(data))
            {
                while (reader.BaseStream.Position < data.Length)
                {
                    uint uin = reader.ReadUInt32(); //gg num
                    uint status = reader.ReadUInt32(); //status
                    uint features = reader.ReadUInt32(); //features
                    reader.ReadUInt32(); //remote ip (not used)
                    reader.ReadUInt16(); //remote port (not used)
                    byte imageSize = reader.ReadByte(); //image size KB
                    reader.ReadByte(); // unknown
                    uint flags = reader.ReadUInt32(); //flags
                    uint descSize = reader.ReadUInt32(); //description size
                    byte[] desc = reader.ReadBytes((int)descSize); //description

                    ContactInfo contact = new ContactInfo();
                    contact.Uin = uin;
                    bool isDesc = false;
                    contact.Status = Utils.ToPublicStatus(status, out isDesc);
                    contact.Features = features;
                    contact.MaxImageSize = imageSize;
                    contact.Flags = flags;
                    if (isDesc) contact.Description = Encoding.UTF8.GetString(desc);

                    contacts.Add(contact);
                }
            }
        }

        public static byte[] WriteLogin(uint uin, string password, uint passwordSeed, Status status, string description)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                byte[] ver = Encoding.ASCII.GetBytes("Gadu-Gadu Client build 10.0.0.10450");
                byte[] desc = Encoding.UTF8.GetBytes(description);
                writer.Write(uin); //gg num
                writer.Write('p'); writer.Write('l'); //language
                writer.Write(Container.GG_LOGIN_HASH_SHA1); //hash type
                writer.Write(Utils.CalculateSHA1Hash(password, passwordSeed)); //pass hash
                writer.Write(Utils.ToInternalStatus(status, (description != string.Empty))); //status
                writer.Write((uint)0); //flags
                writer.Write(Container.GG_LOGIN_FLAG_MSGTYPE_80 | Container.GG_LOGIN_FLAG_STATUSTYPE_80 | Container.GG_LOGIN_FLAG_DNDFFC | Container.GG_LOGIN_FLAG_LOGINFAILEDTYPE | Container.GG_LOGIN_FLAG_UNKNOWN | Container.GG_LOGIN_FLAG_SENDMSGACK); //features
                //writer.Write((uint)0x00000367);
                writer.Write((uint)0); //local ip (not used)
                writer.Write((ushort)0); //local port (not used)
                writer.Write((uint)0); //external ip (not used)
                writer.Write((ushort)0); //external port (not used)
                writer.Write((byte)255); //image size
                writer.Write((byte)0x64); //unknown

                writer.Write((uint)ver.Length); //version length string
                writer.Write(ver); //version

                writer.Write((uint)desc.Length); //description length
                if (description != string.Empty) writer.Write(desc); //description

                return BuildHeader(Container.GG_LOGIN80, writer.Data);
            }
        }
        public static byte[] WriteContactList(List<ContactInfo> contacts, ref int remainingStartOffset)
        {
            int toSend = Math.Min(400, contacts.Count - remainingStartOffset);        

            using (PacketWriter writer = new PacketWriter())
            {
                for (int i = remainingStartOffset; i < toSend + remainingStartOffset; i++)
                {
                    writer.Write(contacts[i].Uin); //gg num
                    writer.Write(Utils.ToInternalContactType(contacts[i].Type)); //contact type
                    //writer.Write((uint)0x02);
                }
                remainingStartOffset += toSend;

                return BuildHeader((toSend < 400 ? Container.GG_NOTIFY_LAST : Container.GG_NOTIFY_FIRST), writer.Data);
            }
        }
        public static byte[] WriteEmptyContactList()
        {
            return BuildHeader(Container.GG_LIST_EMPTY, null);
        }
        public static byte[] WriteAddNotify(uint uin, ContactType type)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(uin);
                writer.Write(Utils.ToInternalContactType(type));

                return BuildHeader(Container.GG_ADD_NOTIFY, writer.Data);
            }
        }
        public static byte[] WriteRemoveNotify(uint uin, ContactType type)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(uin);
                writer.Write(Utils.ToInternalContactType(type));

                return BuildHeader(Container.GG_REMOVE_NOTIFY, writer.Data);
            }
        }
        public static byte[] WriteStatus(Status status, string description)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                byte[] desc = Encoding.UTF8.GetBytes(description);

                writer.Write(Utils.ToInternalStatus(status, (description != string.Empty)) | Container.GG_STATUS_DESCR_MASK); //status
                writer.Write(Container.GG_STATUS_FLAG_LINKS_FROM_UNKNOWN); //flags
                writer.Write((uint)desc.Length); //description length
                if (description != string.Empty) writer.Write(desc); //description

                return BuildHeader(Container.GG_NEW_STATUS80, writer.Data);
            }
        }
        public static byte[] WriteSendMessage(uint recipient, string message)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(recipient); //gg num
                writer.Write((uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds); //sequence number = time from 1.1.1970
                writer.Write(Container.GG_CLASS_CHAT); //message class
                byte[] html_msg = Encoding.UTF8.GetBytes(string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">{0}</span>\0", message));
                byte[] plain_msg = Encoding.ASCII.GetBytes(string.Format("{0}\0", message));
                writer.Write((uint)(html_msg.Length + 20)); //plain offset
                writer.Write((uint)(html_msg.Length + plain_msg.Length)); //attrib offset
                writer.Write(html_msg); //html message
                writer.Write(plain_msg); //plain message
                writer.Write((byte)0); //attributes

                return BuildHeader(Container.GG_SEND_MSG80, writer.Data);
            }
        }
        public static byte[] WriteReceiveAck(uint seq)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(seq); //sequence number

                return BuildHeader(Container.GG_RECV_MSG_ACK, writer.Data);
            }
        }

        private static byte[] BuildHeader(uint packetType, byte[] data)
        {
            byte[] ret = new byte[(data == null ? 8 : data.Length + 8)];
            Buffer.BlockCopy(BitConverter.GetBytes(packetType), 0, ret, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)(data == null ? 0 : data.Length)), 0, ret, 4, 4);
            if (data != null) Buffer.BlockCopy(data, 0, ret, 8, data.Length);
            return ret;
        }
    }
}
