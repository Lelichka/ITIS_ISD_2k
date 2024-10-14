using HttpServer.Models;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[ApiController("account")]
public class Accounts
{
    private static readonly string strConnection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
    private static AccountDAO _accountDao = new AccountDAO(strConnection);

    [HttpGet("list")]
    public List<Account> GetAccounts()
    {
        var list = _accountDao.FindAll();
        return list.ToList();
    }
    [HttpGet("accountInfo")]
    public Account GetAccountInfo(int userId)
    {
        var account = _accountDao.GetById(userId);
        return account;
    }

    [HttpGet("list")]
    public Account GetAccountById(int id)
    {
        var account = _accountDao.GetById(id);
        return account;
    }
    
    [HttpPost("create")]
    public bool SaveAccount(string name = "",string password = "")
    {
        var isValid =  Validate(name, password);
        if (isValid) _accountDao.Create(new Account(){Name = name,Password = password});
        return isValid;
    }
    
    [HttpPost("login")]
    public (bool,int?) Login(string name = "",string password = "")
    {
        var account = _accountDao.GetByName(name);
        if (account != null) return (account.Password == password,account.Id);
        return (false,null);
    }

    private bool Validate(string name, string password)
    {
        return !(name == "" || password == "");
    }
}
