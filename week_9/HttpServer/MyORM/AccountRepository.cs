using System.Collections;
using HttpServer.interfaces;
using HttpServer.Models;
using HttpServer.MyORM;

namespace HttpServer.MyORM;

public class AccountRepository : IRepository<Account>
{
    private string StrConnection { get; set; }
    private List<Account> _accounts { get;  }

    public AccountRepository(string strConnection)
    {
        StrConnection = strConnection;
        _accounts = new Database(StrConnection).Select<Account>( "Accounts").ToList();
    }

    public Account? GetById(int id) => _accounts.Where(ac => ac.Id == id).FirstOrDefault();
    public Account? GetByName(string name) => _accounts.Where(ac => ac.Name == name).FirstOrDefault();

    public IEnumerable<Account> FindAll()
    {
        return _accounts;
    }

    public void Create(Account entity)
    {
        new Database(StrConnection).Insert(entity, "Accounts");
    }

    public void Update(Account entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(Account entity)
    {
        new Database(StrConnection).Delete(entity,"Accounts");
    }
}