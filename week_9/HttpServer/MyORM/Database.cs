using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using HttpServer.Models;

namespace HttpServer.MyORM;

public class Database
{
    public IDbConnection _connection = null;
    public IDbCommand _cmd = null;

    public Database(string connectionString)
    {
        this._connection = new SqlConnection(connectionString);
        this._cmd = _connection.CreateCommand();
    }

    protected IEnumerable<T> ExecuteQuery<T>(string query, bool isStoredProc = false)
    {
        var list = new List<T>();
        var t = typeof(T);
        using (_connection)
        {
            if (isStoredProc)
            {
                _cmd.CommandType = CommandType.StoredProcedure;
            }

            _cmd.CommandText = query;
            _connection.Open();
            var reader = _cmd.ExecuteReader();
            while (reader.Read())
            {
                T obj = (T)Activator.CreateInstance(t);
                t.GetProperties().ToList().ForEach(p => { p.SetValue(obj, reader[p.Name]); });
                list.Add(obj);
            }
        }

        return list;
    }

    protected int ExecuteNonQuery(string query, bool isStoredProc = false)
    {
        var noOfAffectedRows = 0;
        using (_connection)
        {
            if (isStoredProc)
            {
                _cmd.CommandType = CommandType.StoredProcedure;
            }

            _cmd.CommandText = query;
            _connection.Open();
            noOfAffectedRows = _cmd.ExecuteNonQuery();
        }

        return noOfAffectedRows;
    }

    protected T ExecuteScalar<T>(string query, bool isStoredProc = false)
    {
        T result = default(T);
        using (_connection)
        {
            if (isStoredProc)
            {
                _cmd.CommandType = CommandType.StoredProcedure;
            }

            _cmd.CommandText = query;
            _connection.Open();
            result = (T)_cmd.ExecuteScalar();
        }

        return result;
    }

    protected Database AddParameter<T>(string name, T value)
    {
        var param = new SqlParameter();
        param.ParameterName = name;
        param.Value = value;
        _cmd.Parameters.Add(param);
        return this;
    }
    
    public IEnumerable<T> Select<T>(string tableName)
    {
        return ExecuteQuery<T>($"select {string.Join(',',typeof(T).GetProperties().Select(p => p.Name).ToList())} from {tableName}");
    }
    public T? GetById<T>(int id,string tableName)
    {
        return ExecuteQuery<T>($"select {string.Join(',',typeof(T).GetProperties().Select(p => p.Name).ToList())} from {tableName} where Id = {id}").FirstOrDefault();
    }
    public T? GetByEmail<T>(string name,string tableName)
    {
        return ExecuteQuery<T>($"select {string.Join(',',typeof(T).GetProperties().Select(p => p.Name).ToList())} from {tableName} where Name = '{name}'").FirstOrDefault();
    }
    public void Insert<T>(T entity, string tableName)
    {
        var properties = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<NotInsertableAttribute>(true) == null).ToList();
        foreach (var property in properties )
        {
            AddParameter($"@{property.Name}", property.GetValue(entity));
        }
        ExecuteNonQuery($"insert into {tableName} values({string.Join(',',properties.Select(p => "@"+p.Name))}) ");
    }
    public void Delete<T>(T entity, string tableName)
    {
        var properties = typeof(T).GetProperties();
        ExecuteNonQuery($"delete from {tableName} where {string.Join(" and ",properties.Select(p => p.Name+"="+p.GetValue(entity)))} ");
    }


}


