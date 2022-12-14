using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class ApiController:Attribute
    {
        public string ControllerName;
        public ApiController (string controllerName = "")
        {
            ControllerName = controllerName;
        }
    }
}
