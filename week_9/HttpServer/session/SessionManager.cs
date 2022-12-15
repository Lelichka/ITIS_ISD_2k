namespace HttpServer.session;
using Microsoft.Extensions.Caching.Memory;

public class SessionManager
{
    private SessionManager()
    {
    }

    private static readonly Lazy<SessionManager> Lazy =
        new(() => new SessionManager());
    public static SessionManager Instance => Lazy.Value;
    
    private MemoryCache _cache = new(new MemoryCacheOptions());

    public Guid CreateSession(int accountId, string login, DateTime createDateTime)
    {
        var guid = Guid.NewGuid();
        var session = new Session(guid, accountId, login, createDateTime);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(120));

        _cache.Set(guid, session, cacheEntryOptions);
        return guid;
    }


    public bool SessionExist(Guid guid)
    {
        return _cache.TryGetValue(guid, out _);
    }

    public Session? GetSessionInfo(Guid guid)
    {
        _cache.TryGetValue(guid, out Session? session);
        return session;
    }
}