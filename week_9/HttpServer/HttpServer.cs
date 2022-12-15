using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Data.SqlClient;
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
        if (listener.IsListening)
        {
            Console.WriteLine("Сервер уже запущен");
            return;
        }
        settings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes(@"D:\ITIS\2022-2023\Inf\HttpServer\ITIS_ORIS_2k\week_9\HttpServer\settings.json"));
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
        if (!listener.IsListening)
        {
            Console.WriteLine("Сервер не запущен");
            return;
        }
        listener.Stop();
        Console.WriteLine("Сервер остановлен");
    }

    private async Task Listen()
    {
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            var buffer = Handle(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                buffer = Encoding.UTF8.GetBytes("404 - not found");
                ShowOutput(context.Response,buffer);
                StopServer();
                return;
            }
            ShowOutput(context.Response,buffer);
        }
    }
    private static void ShowOutput(HttpListenerResponse response, byte[] buffer)
    {
        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        Task.WaitAll();
        output.Write(buffer, 0, buffer.Length);
        output.Close();
        response.Close();
    }
    private byte[] Handle(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        
        response.StatusCode = (int)HttpStatusCode.NotFound;

        byte[] buffer = new byte[]{};

        var handler = new Handler(context,settings);
        handler.FilesHandler(settings.Path, out buffer);

        if (response.StatusCode == (int)HttpStatusCode.NotFound)
            handler.MethodHandler( out buffer);
        return buffer;
    }
    

    
}