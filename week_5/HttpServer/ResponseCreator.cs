using System.Net;
using System.Text;

namespace HttpServer;

public static class ResponseCreator
{
    public static void CreateResponse(string requestStr, string Path,HttpListenerResponse response, HttpListenerContext context)
    {
        byte[] buffer;
        
        if (Directory.Exists(Path))
        {
            if (requestStr[requestStr.Length - 1] != '/')
            {
                switch (System.IO.Path.GetExtension(Path + requestStr))
                {
                    case ".css":
                        response.ContentType = "text/css";
                        break;
                    case ".png":
                        response.ContentType = "image/png";
                        break;
                    case ".svg":
                        response.ContentType = "image/svg+xml";
                        break;
                    case ".js":
                        response.ContentType = "text/javascript";
                        break;
                    case ".gif":
                        response.ContentType = "image/gif";
                        break;
                    case ".ico":
                        response.ContentType = "image/x-icon";
                        break;
                }
                buffer = File.ReadAllBytes(Path + requestStr);
            }
            else
            {
                response.ContentType = "text/html";
                buffer = File.ReadAllBytes(Path + @"\index.html");
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            string err = "404 - not found";
            buffer = Encoding.UTF8.GetBytes(err);
        }
        ShowOutput(response,buffer);
    }
    public static void ShowOutput(HttpListenerResponse response,byte[] buffer)
    {
        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }
}