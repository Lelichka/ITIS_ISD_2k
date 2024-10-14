using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using HttpServer.MyORM;
using HttpServer.session;

namespace HttpServer;

public class Handler
{
    private SessionManager _sessionManager;
    private HttpListenerContext _listenerContext;
    private AccountDAO _accountDao;
    private static string _connectionStr;
    public Handler(HttpListenerContext listenerContext,ServerSettings settings)
    {
        _connectionStr = settings.SqlConnection;
        _sessionManager = SessionManager.Instance;
        _listenerContext = listenerContext;
        _accountDao = new AccountDAO(_connectionStr);
    }
    public void FilesHandler(string Path, out byte[] buffer)
    {
        var response = _listenerContext.Response;
        var request = _listenerContext.Request;
        var requestStr = request.RawUrl;

        buffer = new byte[]{};
        Directory.SetCurrentDirectory(@"D:\ITIS\2022-2023\Inf\HttpServer\ITIS_ORIS_2k\week_9\HttpServer");
        Path = Path + requestStr;


        if (Directory.Exists(Path))
        {
            response.Headers.Set("Content-Type","text/html");
            buffer = File.ReadAllBytes(Path  + @"html\index.html");
            response.StatusCode = (int)HttpStatusCode.OK;
        }
        else if ( File.Exists(Path))
        {
            response.Headers.Set("Content-Type",ContentTypeGetter.GetContentType(Path+requestStr));
            buffer = File.ReadAllBytes(Path);
            response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
    
    public void MethodHandler(out byte[] buffer)
    {
        HttpListenerRequest request = _listenerContext.Request;
        HttpListenerResponse response = _listenerContext.Response;

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
                        var isCookieAndSessionExist = cookie is not null && _sessionManager.SessionExist(Guid.Parse(cookie.Value));
                        if (!isCookieAndSessionExist)
                        {
                            response.StatusCode = 401;
                            return;
                        }
                        break;
                    }
                        
                    case "GetAccountInfo" :
                    {
                        var cookie = request.Cookies["SessionId"];
                        var isCookieAndSessionExist = cookie is not null && _sessionManager.SessionExist(Guid.Parse(cookie.Value));
                        
                        if (isCookieAndSessionExist)
                        {
                            var session = _sessionManager.GetSessionInfo(Guid.Parse(cookie.Value));
                            queryParams = new object[] { session.AccountId};
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
                        {
                            var guid = _sessionManager.CreateSession(res.Item2!.Value,
                                _accountDao.GetById(res.Item2.Value).Name, DateTime.Now);
                            response.SetCookie(new Cookie("SessionId", guid.ToString()));
                            response.StatusCode = (int)HttpStatusCode.OK;
                        }
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