using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Data.SqlClient;
using HttpServer.Models;

namespace HttpServer
{
    [ApiController("account")]
    public class Accounts
    {
        [HttpGet("list")]
        public List<Account> GetAccounts()
        {
            var list = InstrBD.getListAccounts();
            return list;
        }

        [HttpGet("list")]
        public List<Account> GetAccountById(int id)
        {
            var list = InstrBD.getListAccounts();
            if (id >= 0 && id < list.Count)
                return new List<Account>(){ list[id] };
            return list;

        }
        
        [HttpPost("create")]
        public bool SaveAccount(string name = "",string password = "")
        {
            var isValid =  Validate(name, password);
                //if (isValid) InstrBD.AddAccountToBd(name,password);
            return isValid;
        }

        private bool Validate(string name, string password)
        {
            return !(name == "" || password == "");
        }
    }
}
