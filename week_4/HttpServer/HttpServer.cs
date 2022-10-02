using System;
using System.Net;
using System.IO;
using System.Text;

namespace HttpServer;
public class HttpServer
{
    private HttpListener listener;
    
    public HttpServer()
    {
        listener = new HttpListener();
    }
    
    public void StartServer()
    {
        // установка адресов прослушки
        listener.Prefixes.Add("http://localhost:8888/google/");
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

            var fileName = @"google.html";
            byte[] buffer;
            var Dir = new DirectoryInfo(@"D:\ITIS\2022-2023\Inf\HttpServer\HttpServer");
            if (!Dir.GetFiles().Select(f => f.Name).Contains(fileName))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                string err = "not found";
                buffer = Encoding.UTF8.GetBytes(err);
                ShowOutput(response, buffer);
                StopServer();
            }

            buffer = File.ReadAllBytes(@"D:\ITIS\2022-2023\Inf\HttpServer\HttpServer\" + fileName);
            ShowOutput(response, buffer);
        }
    }
    public void ShowOutput(HttpListenerResponse response,byte[] buffer)
    {
        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        // закрываем поток
        output.Close();
    }
}