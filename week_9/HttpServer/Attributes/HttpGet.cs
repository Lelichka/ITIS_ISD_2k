using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class HttpGet : Attribute, HttpCustomMethod
    {
        public string MethodURI { get; set; }

        public HttpGet(string methodURI = "")
        {
            MethodURI = methodURI;
        }
    }
}
