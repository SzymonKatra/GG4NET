using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GG4NET;

namespace GG4NETExample
{
    class Program
    {
        static void Main(string[] args)
        {
            uint uin;
            string password;
            Console.Write("Podaj numer GG: ");
            uin = uint.Parse(Console.ReadLine());
            Console.Write("Podaj hasło: ");
            password = Console.ReadLine();

            GaduGaduClient client = new GaduGaduClient(uin, password);

            client.ServerObtained += client_ServerObtained;
            client.ServerObtainingFailed += client_ServerObtainingFailed;
            client.Connected += client_Connected;
            client.ConnectFailed += client_ConnectFailed;
            client.Logged += client_Logged;
            client.LoginFailed += client_LoginFailed;
            client.Disconnected += client_Disconnected;
            client.MessageReceived += client_MessageReceived;

            client.Connect();

            while (!client.IsLogged) ; // oczekiwanie na zalogowanie

            uint lastUin = 0;
            while (true)
            {
                string[] tokens = Console.ReadLine().Split(':');
                string msg = tokens[0];
                if (tokens.Length > 1)
                {
                    msg = tokens[1];
                    lastUin = uint.Parse(tokens[0]);
                }

                if (lastUin == 0)
                    Console.WriteLine("Podaj numer do którego chcesz wysłać wiadomość");
                else client.SendMessage(lastUin, msg);
            }
        }

        static void client_ServerObtained(object sender, EventArgs e)
        {
            Console.WriteLine("Znaleziono serwer");
        }

        static void client_ServerObtainingFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Błąd podczas szukania serwera");
        }

        static void client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Połączono");
        }

        static void client_ConnectFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Bład podczas łączenia z serwerem GG");
        }

        static void client_Logged(object sender, EventArgs e)
        {
            Console.WriteLine("Zalogowano");
            Console.WriteLine("Aby wysłac wiadomośc wpisz");
            Console.WriteLine("NUMER_GG:WIADOMOSC");
        }

        static void client_LoginFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Błąd podczas logowania");
        }

        static void client_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Rozłączono");
        }

        static void client_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(string.Format("{0}: {1}", e.Uin, e.Message));
        }
    }
}
