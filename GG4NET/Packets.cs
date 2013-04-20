using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace GG4NET
{
    internal static class Packets
    {
        public static void ReadWelcome(byte[] data, out uint seed)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                seed = reader.ReadUInt32(); //seed

                reader.Close();
            }
        }
        public static void ReadReceiveMessage(byte[] data, out uint uin, out uint seq, out DateTime time, out string plainMessage, out string htmlMessage, out byte[] attributes)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                uin = reader.ReadUInt32(); //gg num
                seq = reader.ReadUInt32(); //sequence number = time from 1.1.1970
                time = new DateTime(1970, 1, 1);
                time.AddSeconds(reader.ReadUInt32());
                reader.ReadUInt32(); // message class
                uint plain_offset = reader.ReadUInt32(); //plain offset
                uint attrib_offset = reader.ReadUInt32(); //attributes offset
                htmlMessage = Encoding.UTF8.GetString(reader.ReadBytes((int)(plain_offset - 25))); //read html message
                plainMessage = Encoding.GetEncoding("windows-1250").GetString(reader.ReadBytes((int)(attrib_offset - plain_offset))); //read plain message
                attributes = reader.ReadBytes((int)(data.Length - reader.BaseStream.Position)); //attributes

                reader.Close();
            }
        }
        public static void ReadNotifyReply(byte[] data, out List<UserInfo> contacts)
        {
            contacts = new List<UserInfo>();
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

                    UserInfo contact = new UserInfo();
                    contact.Uin = uin;
                    bool isDesc = false;
                    contact.Status = Utils.ToPublicStatus(status, out isDesc);
                    contact.Features = features;
                    contact.MaxImageSize = imageSize;
                    contact.Flags = flags;
                    if (isDesc) contact.Description = Encoding.UTF8.GetString(desc);

                    contacts.Add(contact);
                }

                reader.Close();
            }
        }
        public static void ReadMultiloginInfo(byte[] data, out MultiloginInfo[] infos)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                uint count = reader.ReadUInt32(); //connected clients, structs in packet
                infos = new MultiloginInfo[count];
                for (uint i = 0; i < count; i++)
                {
                    infos[i] = new MultiloginInfo();
                    uint ip = reader.ReadUInt32(); //ip
                    infos[i].IP = new IPAddress(BitConverter.GetBytes(ip));
                    infos[i].Flags = reader.ReadUInt32(); //flags
                    infos[i].Features = reader.ReadUInt32(); //features
                    infos[i].LogonTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(reader.ReadUInt32()); //logon time
                    infos[i].ConnectionId = reader.ReadUInt64(); //connection id
                    reader.ReadUInt32(); // unknown
                    uint clientNameSize = reader.ReadUInt32(); // client name size
                    byte[] clientName = reader.ReadBytes((int)clientNameSize); //client name
                    infos[i].ClientName = Encoding.UTF8.GetString(clientName);
                }

                reader.Close();
            }
        }
        public static void ReadTypingNotify(byte[] data, out uint uin, out TypingNotifyType type, out ushort length)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                length = reader.ReadUInt16(); //type or length
                type = (length >= 1 ? TypingNotifyType.Start : TypingNotifyType.Stop);
                uin = reader.ReadUInt32(); //gg num

                reader.Close();
            }
        }
        public static void ReadPublicDirectoryReply(byte[] data, out PublicDirectoryReply[] persons, out uint nextStart)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                byte repType = reader.ReadByte(); //reply type
                reader.ReadUInt32(); //sequence number

                List<PublicDirectoryReply> listPersons = new List<PublicDirectoryReply>();

                string reply = Encoding.UTF8.GetString(reader.ReadBytes(data.Length - 5));
                string[] parsedReply = reply.Split('\0');

                bool cVal = false;
                string cValType = string.Empty;
                int pos = 0;
                listPersons.Add(new PublicDirectoryReply());
                PublicDirectoryReply person = new PublicDirectoryReply();
                nextStart = 0;
                foreach (string s in parsedReply)
                {
                    if (!cVal)
                    {
                        cValType = s;
                        cVal = true;
                        continue;
                    }

                    switch (cValType)
                    {
                        case "FmNumber": person.Uin = uint.Parse(s); break;

                        case "firstname": person.FirstName = s; break;

                        case "lastname": person.LastName = s; break;

                        case "nickname": person.Nickname = s; break;

                        case "birthyear": person.Birthyear = int.Parse(s); break;

                        case "city": person.City = s; break;

                        case "gender": person.Gender = Utils.ToPublicGender(uint.Parse(s), (repType != Container.GG_PUBDIR50_SEARCH || repType != Container.GG_PUBDIR50_SEARCH_REPLY)); break;

                        case "familyname": person.FamilyName = s; break;

                        case "familycity": person.FamilyCity = s; break;

                        case "FmStatus":
                            bool isDesc = false;
                            person.Status = Utils.ToPublicStatus(uint.Parse(s), out isDesc);
                            break;

                        case "nextstart": nextStart = uint.Parse(s); break;

                        case "": //new person
                            listPersons.Add(new PublicDirectoryReply());
                            person = new PublicDirectoryReply();
                            pos = listPersons.Count - 1;
                            break;
                    }
                    listPersons[pos] = person;
                    cVal = false;
                }
                if (listPersons[pos].Uin <= 0) listPersons.RemoveAt(pos);
                persons = listPersons.ToArray();

                reader.Close();
            }
        }
        public static void ReadXmlMessage(byte[] data, out string message)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                message = Encoding.UTF8.GetString(reader.ReadBytes(data.Length));

                reader.Close();
            }
        }
        public static void ReadUserListReply(byte[] data, out byte replyType, out uint listVersion, out ContactListType formatType, out string reply)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                replyType = reader.ReadByte(); //type
                listVersion = reader.ReadUInt32(); //list version
                formatType = Utils.ToPublicContactListFormat(reader.ReadByte()); //format type
                reader.ReadByte(); //unknown
                byte[] rep = reader.ReadBytes(data.Length - 7); //reply

                //if (formatType == ContactListType.XML)
                //{
                    using (MemoryStream memStream = new MemoryStream(rep))
                    {
                        //we must skip first two bytes
                        //thanks to http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
                        byte[] buff = new byte[2];
                        memStream.Read(buff, 0, 2);

                        using (DeflateStream dStream = new DeflateStream(memStream, CompressionMode.Decompress))
                        {
                            byte[] buffer = new byte[16384];
                            int len;
                            PacketWriter output = new PacketWriter();
                            while ((len = dStream.Read(buffer, 0, Math.Min(rep.Length, buffer.Length))) > 0)
                            {
                                output.Write(buffer, 0, len);
                            }
                            //reply = Encoding.UTF8.GetString(output.Data);
                            reply = (formatType == ContactListType.XML ? Encoding.UTF8 : Encoding.GetEncoding("windows-1250")).GetString(output.Data);
                            output.Close();

                            dStream.Close();
                        }
                    }
                //}
                //else reply = Encoding.GetEncoding("windows-1250").GetString(rep);


                //reply = (formatType == ContactListType.XML ? Encoding.UTF8 : Encoding.GetEncoding("windows-1250")).GetString(rep);

                reader.Close();
            }
        }
        public static void ReadUserListVersion(byte[] data, out uint version)
        {
            using (PacketReader reader = new PacketReader(data))
            {
                version = reader.ReadUInt32(); //version

                reader.Close();
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
                writer.Write(Container.GG_LOGIN_FLAG_MSGTYPE_80 | Container.GG_LOGIN_FLAG_STATUSTYPE_80 | Container.GG_LOGIN_FLAG_DNDFFC | Container.GG_LOGIN_FLAG_LOGINFAILEDTYPE | Container.GG_LOGIN_FLAG_UNKNOWN | Container.GG_LOGIN_FLAG_SENDMSGACK | Container.GG_LOGIN_FLAG_MULTILOGIN | Container.GG_LOGIN_FLAG_TYPINGNOTIF); //features
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
        public static byte[] WriteContactList(List<UserInfo> contacts, ref int remainingStartOffset)
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

                writer.Write(Utils.ToInternalStatus(status, (description != string.Empty))/* | Container.GG_STATUS_DESCR_MASK*/); //status
                writer.Write(Container.GG_STATUS_FLAG_LINKS_FROM_UNKNOWN); //flags
                writer.Write((uint)desc.Length); //description length
                if (description != string.Empty) writer.Write(desc); //description

                return BuildHeader(Container.GG_NEW_STATUS80, writer.Data);
            }
        }
        public static byte[] WriteSendMessage(uint recipient, string plainMessage, string htmlMessage, byte[] attributes)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(recipient); //gg num
                writer.Write((uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds); //sequence number = time from 1.1.1970
                writer.Write(Container.GG_CLASS_CHAT); //message class
                byte[] html_msg = Encoding.UTF8.GetBytes(string.Format("{0}\0", htmlMessage));
                byte[] plain_msg = Encoding.GetEncoding("windows-1250").GetBytes(string.Format("{0}\0", plainMessage));
                writer.Write((uint)(html_msg.Length + 19)); //plain offset
                writer.Write((uint)(html_msg.Length + plain_msg.Length)); //attrib offset
                writer.Write(html_msg); //html message
                writer.Write(plain_msg); //plain message
                if (attributes != null) writer.Write(attributes); //attributes

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
        public static byte[] WritePing()
        {
            return BuildHeader(Container.GG_PING, null);
        }
        public static byte[] WriteDisconnectMultiloginSession(ulong connectionId)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(connectionId);

                return BuildHeader(Container.GG_MULTILOGON_DISCONNECT, writer.Data);
            }
        }
        public static byte[] WriteTypingNotify(uint uin, TypingNotifyType type, ushort length = 0)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write((length >= 1 ? length : Utils.ToInternalTypingNotify(type))); //type or length
                writer.Write(uin); //gg num

                return BuildHeader(Container.GG_TYPING_NOTIFY, writer.Data);
            }
        }
        public static byte[] WritePublicDirectoryRequest(byte requestType, uint uin, string firstName, string lastName, string nickname, int startBirthyear, int stopBirthyear, string city, Gender gender, bool activeOnly, string familyName, string familyCity, uint start)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(requestType); //request type
                writer.Write((uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds); //sequence number

                if (uin > 0) //uin
                {
                    writer.Write(Encoding.ASCII.GetBytes("FmNumber"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(uin.ToString()));
                    writer.Write('\0');
                }
                if (firstName != null && firstName != string.Empty) //first name
                {
                    writer.Write(Encoding.ASCII.GetBytes("firstname"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(firstName));
                    writer.Write('\0');
                }
                if (lastName != null && lastName != string.Empty) //last name
                {
                    writer.Write(Encoding.ASCII.GetBytes("lastname"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(lastName));
                    writer.Write('\0');
                }
                if (nickname != null && nickname != string.Empty) //nickname
                {
                    writer.Write(Encoding.ASCII.GetBytes("nickname"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(nickname));
                    writer.Write('\0');
                }
                if (startBirthyear > 0) //birthyear
                {
                    writer.Write(Encoding.ASCII.GetBytes("birthyear"));
                    writer.Write('\0');
                    writer.Write(Encoding.ASCII.GetBytes(startBirthyear.ToString()));
                    if (stopBirthyear > 0 && startBirthyear != stopBirthyear)
                    {
                        writer.Write(' ');
                        writer.Write(Encoding.ASCII.GetBytes(stopBirthyear.ToString()));
                    }
                    writer.Write('\0');
                }
                if (city != null || city != string.Empty) //city
                {
                    writer.Write(Encoding.ASCII.GetBytes("city"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(city));
                    writer.Write('\0');
                }
                if (gender != Gender.None) //gender
                {
                    writer.Write(Encoding.ASCII.GetBytes("gender"));
                    writer.Write('\0');
                    writer.Write(Encoding.ASCII.GetBytes(Utils.ToInternalGender(gender, (requestType != Container.GG_PUBDIR50_SEARCH)).ToString()));
                    writer.Write('\0');
                }
                if (activeOnly) //active
                {
                    writer.Write(Encoding.ASCII.GetBytes("ActiveOnly"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(Container.GG_PUBDIR50_ACTIVE_TRUE.ToString()));
                    //writer.Write(Encoding.UTF8.GetBytes((activeOnly ? Container.GG_PUBDIR50_ACTIVE_TRUE.ToString() : Container.GG_PUBDIR50_ACTIVE_FALSE.ToString())));
                    writer.Write('\0');
                }
                if (familyName != null && familyName != string.Empty) //family name
                {
                    writer.Write(Encoding.ASCII.GetBytes("familyname"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(familyName));
                    writer.Write('\0');
                }
                if (familyCity != null && familyCity != string.Empty) //family city
                {
                    writer.Write(Encoding.ASCII.GetBytes("familycity"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(familyCity));
                    writer.Write('\0');
                }
                if (start > 0) //start uin
                {
                    writer.Write(Encoding.ASCII.GetBytes("fmstart"));
                    writer.Write('\0');
                    writer.Write(Encoding.UTF8.GetBytes(start.ToString()));
                    writer.Write('\0');
                }

                return BuildHeader(Container.GG_PUBDIR50_REQUEST, writer.Data);
            }
        }
        public static byte[] WriteUserListRequest(byte requestType, uint listVersion, ContactListType formatType, string request)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write(requestType); //type
                writer.Write(listVersion); //list version
                writer.Write(Utils.ToInternalContactListFormat(formatType)); //format type
                writer.Write((byte)0x01); // unknown
                if (request != null && request != string.Empty) //request
                {
                    if (formatType == ContactListType.XML)
                    {
                        byte[] utfReq = Encoding.UTF8.GetBytes(request);
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            //2 first bytes
                            byte[] headerBuffer = new byte[] { 120, 218 };
                            memStream.Write(headerBuffer, 0, headerBuffer.Length);

                            using (DeflateStream dStream = new DeflateStream(memStream, CompressionMode.Compress))
                            {
                                dStream.Write(utfReq, 0, utfReq.Length);
                            }
                            writer.Write(memStream.ToArray());
                            memStream.Close();
                        }
                    }
                    else
                    {
                        writer.Write(Encoding.GetEncoding("windows-1250").GetBytes(request));
                    }
                }

                return BuildHeader(Container.GG_USERLIST100_REQUEST, writer.Data);
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