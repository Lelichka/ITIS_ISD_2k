using System.Data.SqlClient;
using HttpServer.Models;

namespace HttpServer;

public class InstrBD
{
    public static List<Account> getListAccounts()
    {
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
        string sqlExpression = @"SELECT * FROM Accounts";
        var list = new List<Account>();
 
        // Создание подключения
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int id;
                    int.TryParse(reader.GetValue(0).ToString(),out id);
                    var name = reader.GetValue(1).ToString();
                    var password = reader.GetValue(2).ToString();
                    var account = new Account( name, password);
                    list.Add(account);
                }
            }
            reader.Close();
        }

        return list;

    }
    public static void AddAccountToBd(string name,string password)
    {
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
        string sqlExpression = $"insert into Accounts (Name,Password) values ('{name}', '{password}')";
        
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            connection.Open();
            command.ExecuteNonQuery(); 
        }
    }
}