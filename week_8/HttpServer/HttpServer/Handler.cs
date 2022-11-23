using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace HttpServer;

public static class Handler
{
    public static void FailsHandler(string Path, HttpListenerContext context, out byte[] buffer)
    {
        var response = context.Response;
        var request = context.Request;
        var requestStr = request.RawUrl;

        buffer = new byte[]{};

        if (Directory.Exists(Path))
        {
            if (File.Exists(Path + requestStr) || Directory.Exists(Path + requestStr))
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
                    default:
                        response.ContentType = "text/html";
                        break;
                }

                if (requestStr != "/")
                    buffer = File.ReadAllBytes(Path + requestStr);
                else
                    buffer = File.ReadAllBytes(Path + @"/index.html");
                response.StatusCode = (int)HttpStatusCode.OK;
            }
        }
    }
    
    public static void MethodHandler(HttpListenerContext context, out byte[] buffer)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        buffer = new byte[]{};
        if (request.Url.Segments.Length < 2) return;
        
        string[] strParams = request.Url
                                .Segments
                                .Skip(2)
                                .Select(s => s.Replace("/", ""))
                                .ToArray();

        var assembly = Assembly.GetExecutingAssembly();
        
        string controllerName = request.Url.Segments[1].Replace("/", "");
        var controller = assembly.GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(ApiController)))
            .FirstOrDefault(c => 
                (((ApiController)c.GetCustomAttribute(typeof(ApiController))).ControllerName == controllerName.ToLower() ||
                c.Name.ToLower() == controllerName.ToLower()));

        if (controller == null) return;

        MethodInfo? method = null;
        object[] queryParams = null;
        object? ret;
        switch (request.HttpMethod)
        {
            case "GET":
                var methodsGet = controller.GetMethods().Where(t =>
                    t.GetCustomAttributes(true).Any(attr => attr.GetType().Name == "HttpGet"
                                                            && ((HttpCustomMethod)attr).MethodURI == strParams[0]));
                method = methodsGet.Where(method => method.GetParameters().Length == strParams.Length - 1).FirstOrDefault();
                if (method == null) return;
                
                strParams = strParams.Skip(1).ToArray();
                
                queryParams = method.GetParameters()
                    .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                    .ToArray();
                
                ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
                response.ContentType = "Application/json";
                response.StatusCode = (int)HttpStatusCode.OK;
                break;
            case "POST":
                method = controller.GetMethods().Where(t =>
                    t.GetCustomAttributes(true).Any(attr => attr.GetType().Name == $"HttpPost" 
                                                            && ((HttpCustomMethod)attr).MethodURI == strParams[0])).FirstOrDefault();
                if (method == null) return;
                var bodyParams = GetRequestBody(request);
                queryParams = method.GetParameters()
                    .Select((p, i) => Convert.ChangeType(bodyParams[i], p.ParameterType))
                    .ToArray();
                ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
                if ((bool)ret && method.Name == "SaveAccount" )
                    response.Redirect("http://store.steampowered.com/");
                else
                    response.Redirect("http://localhost:8888/");
                break;
            default: 
                ret = null;
                break;
        }
        buffer = (request.HttpMethod == HttpMethod.Post.Method)?new byte[]{}:Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
    }
    private static object[] GetRequestBody(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
            return null;
        using (Stream body = request.InputStream)
        {
            using (var reader = new StreamReader(body, request.ContentEncoding))
                return reader.ReadToEnd()
                    .Split('&')
                    .Select(elem => elem.Split('=')[1])
                    .Select(elem => (object)elem)
                    .ToArray();
        }
    }
}