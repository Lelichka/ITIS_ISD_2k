using HttpServer.Models;

namespace HttpServer.MyORM;

public class AccountDAO 
{
    private string StrConnection { get; set; }

    public AccountDAO(string strConnection)
    {
        StrConnection = strConnection;
    }
    public Account GetById(int id)
    {
        return new Database(StrConnection).GetById<Account>(id, "Account");
    }

    public IEnumerable<Account> FindAll()
    {
        return new Database(StrConnection).Select<Account>( "Account");
    }

    public void Create(Account entity)
    {
        new Database(StrConnection).Insert(entity, "Account");
    }

    public void Update(Account entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(Account entity)
    {
        new Database(StrConnection).Delete(entity,"Account");
    }
}