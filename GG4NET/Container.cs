using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG4NET
{
    internal static class Container
    {
        public const uint GG_WELCOME = 0x0001;
        public const uint GG_LOGIN80 = 0x0031;
        public const uint GG_LOGIN80_OK = 0x0035;
        public const uint GG_LOGIN80_FAILED = 0x0043;

        public const uint GG_DISCONNECTING = 0x000b;

        public const uint GG_NEW_STATUS80 = 0x0038;

        public const uint GG_PONG = 0x0007;
        public const uint GG_PING = 0x0008;

        public const uint GG_RECV_MSG80 = 0x002e;
        public const uint GG_RECV_MSG_ACK = 0x0046;
        public const uint GG_SEND_MSG80 = 0x002d;

        public const uint GG_RECV_OWN_MSG = 0x005A;
        public const uint GG_MULTILOGON_INFO = 0x005B;

        public const uint GG_NEED_EMAIL = 0x0014;

        public const uint GG_LIST_EMPTY = 0x0012;
        public const uint GG_NOTIFY_FIRST = 0x000f;
        public const uint GG_NOTIFY_LAST = 0x0010;
        public const uint GG_NOTIFY_REPLY80 = 0x0037;
        public const uint GG_STATUS80 = 0x0036;
        public const uint GG_ADD_NOTIFY = 0x000d;
        public const uint GG_REMOVE_NOTIFY = 0x000e;

        public const uint GG_TYPING_NOTIFY = 0x0059;

        public const uint GG_PUBDIR50_REQUEST = 0x0014;
        public const uint GG_PUBDIR50_REPLY = 0x000e;


        public const byte GG_LOGIN_HASH_GG32 = 0x01;
        public const byte GG_LOGIN_HASH_SHA1 = 0x02;

        public const uint GG_STATUS_NOT_AVAIL = 0x0001;
        public const uint GG_STATUS_NOT_AVAIL_DESCR = 0x0015;
        public const uint GG_STATUS_FFC = 0x0017;
        public const uint GG_STATUS_FFC_DESCR = 0x0018;
        public const uint GG_STATUS_AVAIL = 0x0002;
        public const uint GG_STATUS_AVAIL_DESCR = 0x0004;
        public const uint GG_STATUS_BUSY = 0x0003;
        public const uint GG_STATUS_BUSY_DESCR = 0x0005;
        public const uint GG_STATUS_DND = 0x0021;
        public const uint GG_STATUS_DND_DESCR = 0x0022;
        public const uint GG_STATUS_INVISIBLE = 0x0014;
        public const uint GG_STATUS_INVISIBLE_DESCR = 0x0016;
        public const uint GG_STATUS_BLOCKED = 0x0006;
        public const uint GG_STATUS_IMAGE_MASK = 0x0100;
        public const uint GG_STATUS_ADAPT_STATUS_MASK = 0x0400;
        public const uint GG_STATUS_DESCR_MASK = 0x4000;
        public const uint GG_STATUS_FRIENDS_MASK = 0x8000;

        public const uint GG_LOGIN_FLAG_NOTIFYTYPE_77 = 0x00000001;
        public const uint GG_LOGIN_FLAG_MSGTYPE_80 = 0x00000002;
        public const uint GG_LOGIN_FLAG_STATUSTYPE_80 = 0x00000004;
        public const uint GG_LOGIN_FLAG_DNDFFC = 0x00000010;
        public const uint GG_LOGIN_FLAG_GRAPHICSTATUSES = 0x00000020;
        public const uint GG_LOGIN_FLAG_LOGINFAILEDTYPE = 0x00000040;
        public const uint GG_LOGIN_FLAG_UNKNOWN = 0x00000100;
        public const uint GG_LOGIN_FLAG_ADDINFO = 0x00000200;
        public const uint GG_LOGIN_FLAG_SENDMSGACK = 0x00000400;
        public const uint GG_LOGIN_FLAG_TYPINGNOTIF = 0x00002000;
        public const uint GG_LOGIN_FLAG_MULTILOGIN = 0x00004000;

        public const uint GG_STATUS_FLAG_AUDIO = 0x00000001;
        public const uint GG_STATUS_FLAG_VIDEO = 0x00000002;
        public const uint GG_STATUS_FLAG_MOBILE = 0x00100000;
        public const uint GG_STATUS_FLAG_LINKS_FROM_UNKNOWN = 0x00800000;

        public const uint GG_CLASS_CHAT = 0x0008;

        public const uint GG_USER_OFFLINE = 0x01;
        public const uint GG_USER_NORMAL = 0x03;
        public const uint GG_USER_BLOCKED = 0x04;

        public const ushort GG_TYPING_NOTIFY_TYPE_START = 0x0001;
        public const ushort GG_TYPING_NOTIFY_TYPE_STOP = 0x0000;

        public const byte GG_PUBDIR50_WRITE = 0x01;
        public const byte GG_PUBDIR50_READ = 0x02;
        public const byte GG_PUBDIR50_SEARCH = 0x03;
        public const byte GG_PUBDIR50_SEARCH_REPLY = 0x05;

        public const uint GG_PUBDIR50_GENDER_FEMALE = 1;
        public const uint GG_PUBDIR50_GENDER_MALE = 2;
        public const uint GG_PUBDIR50_GENDER_SET_FEMALE = 2;
        public const uint GG_PUBDIR50_GENDER_SET_MALE = 1;

        public const uint GG_PUBDIR50_ACTIVE_TRUE = 1;
        public const uint GG_PUBDIR50_ACTIVE_FALSE = 0;
    }
}
