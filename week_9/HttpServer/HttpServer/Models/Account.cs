using System.ComponentModel;

namespace HttpServer.Models;

public class Account 
{
    [NotInsertable]
    public int Id { get; set; }
    public string Name { get; set; } 
    public string Password { get; set; } 
}