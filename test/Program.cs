using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GG4NET;
using System.Net;
using System.Threading;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            GaduGaduClient gg = new GaduGaduClient(44213806, "testing123");
            gg.Connected += gg_Connected;
            gg.Disconnected += gg_Disconnected;
            gg.Logged += gg_Logged;
            gg.ServerObtained += gg_ServerObtained;
            gg.MessageReceived += gg_MessageReceived;
            gg.Description = "heheszka";
            gg.Connect(gg_ConnectFailed, gg_LoginFailed, gg_ServerObtainingFailed);
            while (true)
            {
                try
                {
                    gg.SendMessage(8851138, Console.ReadLine());
                }
                catch { gg.Disconnect(); break; }
            }
        }

        static void gg_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.Write(e.Sender + ": " + e.Message);
        }

        static void gg_ConnectFailed(object sender, EventArgs e)
        {
            Console.WriteLine("nie udalo sie polaczyc");
        }
        static void gg_LoginFailed(object sender, EventArgs e)
        {
            Console.WriteLine("logowanie nie udalo sie");  
        }
        static void gg_ServerObtainingFailed(object sender, EventArgs e)
        {
            Console.WriteLine("nie ma serwera");
        }
        static void gg_ServerObtained(object sender, EventArgs e)
        {
            Console.WriteLine("jest serwer");
        }
        static void gg_LoggedOut(object sender, EventArgs e)
        {
            Console.WriteLine("wylogowano");
        }
        static void gg_Logged(object sender, EventArgs e)
        {
            Console.WriteLine("zalogowano");
            System.Threading.Thread.Sleep(500);
        }
        static void gg_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("rozlaczono");
        }
        static void gg_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("polaczono");
        }
    }
}
