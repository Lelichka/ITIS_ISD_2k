using HttpServer.Models;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[ApiController("account")]
public class Accounts
{
    private readonly string strConnection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
    [HttpGet("list")]
    public List<Account> GetAccounts()
    {
        var list = new AccountRepository(strConnection).FindAll();
        return list.ToList();
    }

    [HttpGet("list")]
    public Account GetAccountById(int id)
    {
        var account = new AccountRepository(strConnection).GetById(id);
        return account;
    }
    
    [HttpPost("create")]
    public bool SaveAccount(string name = "",string password = "")
    {
        var isValid =  Validate(name, password);
        if (isValid) new AccountRepository(strConnection).Create(new Account(){Name = name,Password = password});
        return isValid;
    }
    
    [HttpPost("login")]
    public (bool,int?) Login(string name = "",string password = "")
    {
        var account = new AccountRepository(strConnection).GetByName(name);
        if (account != null) return (account.Password == password,account.Id);
        return (false,null);
    }

    private bool Validate(string name, string password)
    {
        return !(name == "" || password == "");
    }
}
