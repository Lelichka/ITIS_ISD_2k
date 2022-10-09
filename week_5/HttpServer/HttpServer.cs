using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HttpServer;
public class HttpServer
{
    private HttpListener listener;
    private ServerSettings? settings;
    
    public HttpServer()
    {
        listener = new HttpListener();
    }

    public void StartServer()
    {
        settings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes(@"D:\ITIS\2022-2023\Inf\HttpServer\HttpServer\settings.json"));
        listener.Prefixes.Clear();
        listener.Prefixes.Add($"http://localhost:{settings.Port}/");
        listener.Start();
        Console.WriteLine("Сервер запущен");
        Console.WriteLine("Ожидание подключений...");
        while (true)
        {
            Listen();
            Program.UserCommand(this);
        }
    }

    public void StopServer()
    {
        listener.Stop();
        Console.WriteLine("Сервер остановлен");
    }

    private async Task Listen()
    {
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;

            HttpListenerResponse response = context.Response;

            var requestStr = request.RawUrl;
            byte[] buffer;
            ResponseCreator.CreateResponse(requestStr,settings.Path,response,context);
            
            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
                StopServer();
        }
    }
}