using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Attributes;

namespace URBE.Pokemon.API.Services;

[RegisterUrbeService(ServiceLifetime.Scoped)]
public class SessionManager
{
    private readonly UrbeContext Db;

    public SessionManager(UrbeContext db)
    {
        Db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Session?> FetchSession(Id<Session> sessionId)
    {
        var session = await Db.Sessions.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session is null)
            return null;

        Debug.Assert(session?.User is not null);

        var dtnow = DateTimeOffset.Now;
        await Db.Sessions.Where(x => x.Id == sessionId).ExecuteUpdateAsync(x => x.SetProperty(x => x.LastUsed, dtnow));

        return session;
    }

    public async Task<Session> CreateSession(User user)
    {
        var ns = new Session()
        {
            Expiration = string.IsNullOrWhiteSpace(user.PasswordHash) ? Program.Settings.AnonymousSessionTimeout : Program.Settings.UserSessionTimeout,
            CreatedDate = DateTimeOffset.Now,
            Id = Id<Session>.New(),
            LastUsed = DateTimeOffset.Now,
            UserId = user.Id
        };

        Db.Sessions.Add(ns);
        await Db.SaveChangesAsync();
        return ns;
    }
}
