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
                seed = reader.ReadUInt32();
            }
        }
        public static void ReadReceiveMessage(byte[] data, out uint sender, out uint seq, out DateTime time, out string plainMessage, out string htmlMessage)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                sender = reader.ReadUInt32();
                seq = reader.ReadUInt32();
                time = new DateTime(1970, 1, 1);
                time.AddSeconds(reader.ReadUInt32());
                reader.ReadUInt32();
                uint plain_offset = reader.ReadUInt32();
                uint attrib_offset = reader.ReadUInt32();
                //List<byte> html=new List<byte>();
                //char current = ' ';
                //while (current != '\0')
                //{
                //    current = reader.ReadChar();
                //    html.Add(Convert.ToByte(current));
                //}
                //htmlMessage = Encoding.UTF8.GetString(html.ToArray());
                htmlMessage = Encoding.UTF8.GetString(reader.ReadBytes((int)(plain_offset - 24)));
                plainMessage = Encoding.ASCII.GetString(reader.ReadBytes((int)(attrib_offset - plain_offset)));
            }
        }

        public static byte[] WriteLogin(uint uin, string password, uint passwordSeed, Status status, string description)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                byte[] ver = Encoding.ASCII.GetBytes("Gadu-Gadu Client build 10.0.0.10450");
                byte[] desc = Encoding.ASCII.GetBytes(description);
                writer.Write(uin);
                writer.Write('p'); writer.Write('l');
                writer.Write(Container.GG_LOGIN_HASH_SHA1);
                writer.Write(Utils.CalculateSHA1Hash(password, passwordSeed));
                writer.Write(Utils.ToInternalStatus(status, (description != string.Empty)));
                writer.Write((uint)0);
                writer.Write(Container.GG_LOGIN_FLAG_MSGTYPE_80 | Container.GG_LOGIN_FLAG_STATUSTYPE_80 | Container.GG_LOGIN_FLAG_DNDFFC | Container.GG_LOGIN_FLAG_LOGINFAILEDTYPE | Container.GG_LOGIN_FLAG_UNKNOWN | Container.GG_LOGIN_FLAG_SENDMSGACK);
                //writer.Write((uint)0x00000367);
                writer.Write((uint)0);
                writer.Write((ushort)0);
                writer.Write((uint)0);
                writer.Write((ushort)0);
                writer.Write((byte)255);
                writer.Write((byte)0x64);

                writer.Write((uint)ver.Length);
                writer.Write(ver);
                //foreach (char c in ver) writer.Write(c);

                writer.Write((uint)(description == string.Empty ? 0 : desc.Length));
                if (description != string.Empty) writer.Write(desc);
                //if (description != string.Empty) writer.Write(description); else writer.Write((uint)0);

                return BuildHeader(Container.GG_LOGIN80, writer.Data);
            }
        }
        public static byte[] WriteEmptyList()
        {
            return BuildHeader(Container.GG_LIST_EMPTY, null);
        }
        public static byte[] WriteStatus(Status status, string description)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                byte[] desc = Encoding.ASCII.GetBytes(description);

                writer.Write(Utils.ToInternalStatus(status, (description != string.Empty)));
                writer.Write(Container.GG_STATUS_FLAG_LINKS_FROM_UNKNOWN);
                writer.Write((uint)(description == string.Empty ? 0 : desc.Length));
                if (description != string.Empty) writer.Write(desc);

                return BuildHeader(Container.GG_NEW_STATUS80, writer.Data);
            }
        }
        public static byte[] WriteSendMessage(uint recipient, string message)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(recipient);
                writer.Write((uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds);
                writer.Write(Container.GG_CLASS_CHAT);
                byte[] html_msg = Encoding.UTF8.GetBytes(string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">{0}</span>\0", message));
                byte[] plain_msg = Encoding.ASCII.GetBytes(string.Format("{0}\0", message));
                writer.Write((uint)(html_msg.Length + 20)); //plain offset
                writer.Write((uint)(html_msg.Length + plain_msg.Length)); //attrib offset
                writer.Write(html_msg);
                writer.Write(plain_msg);
                writer.Write((byte)0);

                return BuildHeader(Container.GG_SEND_MSG80, writer.Data);
            }
        }
        public static byte[] WriteReceiveAck(uint seq)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(seq);

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
