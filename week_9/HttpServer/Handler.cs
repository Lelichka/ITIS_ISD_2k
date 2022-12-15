using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace HttpServer;

public static class Handler
{
    public static void FilesHandler(string Path, HttpListenerContext context, out byte[] buffer)
    {
        var response = context.Response;
        var request = context.Request;
        var requestStr = request.RawUrl;

        buffer = new byte[]{};
        Directory.SetCurrentDirectory(@"D:\ITIS\2022-2023\Inf\HttpServer\ITIS_ORIS_2k\week_9\HttpServer");
        Path = Path + requestStr;


        if (Directory.Exists(Path ) )
        {
            response.Headers.Set("Content-Type","text/html");
            buffer = File.ReadAllBytes(Path  + @"html\index.html");
            response.StatusCode = (int)HttpStatusCode.OK;
        }
        else if ( File.Exists(Path ))
        {
            response.Headers.Set("Content-Type",ContentTypeGetter.GetContentType(Path+requestStr));
            buffer = File.ReadAllBytes(Path);
            response.StatusCode = (int)HttpStatusCode.OK;
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
                if (methodsGet == null) return;
                method = methodsGet.Where(method => method.GetParameters().Length == strParams.Length - 1).FirstOrDefault();
                if (method == null) method = methodsGet.First();

                strParams = strParams.Skip(1).ToArray();

                queryParams = (method.GetParameters().Length == strParams.Length)
                    ? method.GetParameters()
                        .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                        .ToArray()
                    : null;
                
                

                switch (method.Name)
                {
                    case "GetAccounts":
                    {
                        var cookie = request.Cookies["SessionId"];
                        if (!(cookie != null && cookie.Value.Split('_')[0] == "IsAuthorize:true"))
                        {
                            response.StatusCode = 401;
                            return;
                        }
                        break;
                    }
                        
                    case "GetAccountInfo" :
                    {
                        var cookie = request.Cookies["SessionId"];
                        var cookieSplit = cookie.Value.Split('_');
                        if (cookie != null && cookieSplit[0] == "IsAuthorize:true")
                        {
                            queryParams = new object[] { int.Parse(cookieSplit[1].Split('=')[1]) };
                        }
                        else
                        {
                            response.StatusCode = 401;
                            return;
                        }
                        break;
                    }
                }
                ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
                response.ContentType = "Application/json";
                response.StatusCode = (int)HttpStatusCode.OK;
                buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
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
                switch (method.Name)
                {
                    case "SaveAccount":
                    {
                        
                        if ((bool)ret)
                            response.Redirect("http://store.steampowered.com/");
                        else 
                            response.Redirect("http://localhost:8888/");
                        break;
                    }
                    case "Login":
                    {
                        var res = ((bool, int?))ret;
                        if (res.Item1)
                            response.SetCookie(new Cookie("SessionId", $"IsAuthorize:true_Id={res.Item2.ToString()}"));
                        response.StatusCode = (int)HttpStatusCode.OK;
                        break;
                    }
                    default:
                    {
                        buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
                        response.StatusCode = (int)HttpStatusCode.OK;
                        break;
                    }
                }
                break;
        }
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