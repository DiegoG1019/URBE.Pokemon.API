using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Serilog;
using URBE.Pokemon.API.Attributes;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Filters;

public class AuthenticateSessionAttribute : ServiceFilterAttribute
{
    public AuthenticateSessionAttribute() : base(typeof(SessionAuthenticationFilter)) { }
}

[RegisterUrbeService(ServiceLifetime.Scoped)]
public class SessionAuthenticationFilter : UrbeFilter, IAsyncAuthorizationFilter
{
    public const string SessionIdCookie = "session-id";
    private readonly SessionManager Sessions;
    private readonly UserManager Users;

    public SessionAuthenticationFilter(SessionManager sessions, UserManager users)
    {
        Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        Users = users;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var log = CreateLogger(httpContext);

        if (httpContext.Features.Get<User>() is not null || context.Result is not null)
            return;

        Session? session;
        User? user;

        log.Verbose("Reading user cookie to authenticate");
        var cookie = httpContext.Request.Cookies.FirstOrDefault(x => x.Key == SessionIdCookie);
        log.Debug("Attempting to authenticate session id {sessionid}", cookie.Value);

        if (string.IsNullOrWhiteSpace(cookie.Value) is false && Guid.TryParse(cookie.Value, out var sguid))
        {
            log.Verbose("Succesfully parsed session id {sessionid}", cookie.Value);
            Id<Session> sessionId = new(sguid);
            session = await Sessions.FetchSession(sessionId);

            user = session?.User;
            if (user is null)
            {
                log.Verbose("No user was found associated to session id {sessionid}", cookie.Value);
                (user, session) = await NewUser(log, httpContext);
            }
            else
                log.Debug("Succesfully authenticated user {user} ({userid}) under session {sessionid}", user.DisplayName, user.Id, cookie.Value);
        }
        else
            (user, session) = await NewUser(log, httpContext);

        Debug.Assert(user is not null);
        Debug.Assert(session is not null);
        httpContext.Features.Set(user);
        httpContext.Features.Set(session);
        httpContext.Response.Cookies.AppendSessionCookie(session, log);
    }

    private async Task<(User, Session)> NewUser(ILogger log, HttpContext httpContext)
    {
        log.Verbose("Creating new anonymous user");
        var u = new User()
        {
            Username = AnonUserNameGenerator.GetName(),
            Id = Id<User>.New()
        };

        await Users.AddUser(u);
        var s = await Sessions.CreateSession(u);

        log.Debug("Created new user {user} ({userid}); and created a new session {sessionid} for them", u.DisplayName, u.Id, s.Id);

        return (u, s);
    }
}
