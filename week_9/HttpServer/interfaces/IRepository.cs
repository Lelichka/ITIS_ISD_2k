using System.Collections;

namespace HttpServer.interfaces;

public interface IRepository<T> 
{
    T GetById(int id);
    IEnumerable<T> FindAll();
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}