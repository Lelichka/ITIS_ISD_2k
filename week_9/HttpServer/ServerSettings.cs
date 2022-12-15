
namespace HttpServer;

public class ServerSettings
{
    public int Port { get; set; } = 8888;
    public string Path { get; set; } = @"static";

    public string SqlConnection { get; set; } =
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
}