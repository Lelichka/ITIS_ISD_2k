using System;
using System.Net;
using System.IO;
using System.Reflection.Metadata;

namespace HttpServer;
 class Program
{
    static void Main(string[] args)
    {
        var server = new HttpServer();
        server.StartServer();
        UserCommand(server);
    }

    public static void UserCommand(HttpServer server)
    {
        while (true)
        {
            var command = Console.ReadLine();
            switch (command)
            {
                case "start":
                    server.StartServer();
                    break;
                case "restart":
                    server.StopServer();
                    server.StartServer();
                    break;
                case "stop":
                    server.StopServer();
                    break;
                default:
                    Console.WriteLine("Неизвестная команда");
                    break;
            }
        }
    }
}