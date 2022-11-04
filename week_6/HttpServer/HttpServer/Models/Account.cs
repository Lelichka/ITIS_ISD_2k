using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Models
{
    public class Account
    {
        public Account(string name, string password)
        {
            Name = name;
            Password = password;
        }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
