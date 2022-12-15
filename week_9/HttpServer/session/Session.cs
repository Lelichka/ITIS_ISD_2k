﻿namespace HttpServer.session;


public class Session
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Login { get; set; }
    public DateTime CreateDateTime { get; set; }

    public Session(int id, int accountId, string login, DateTime createDateTime)
    {
        Id = id;
        AccountId = accountId;
        Login = login;
        CreateDateTime = createDateTime;
    }
}