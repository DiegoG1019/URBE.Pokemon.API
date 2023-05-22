using System.Collections.Concurrent;
using MimeKit.Cryptography;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Models.Database;

namespace URBE.Pokemon.API;

public static class HttpHelpers
{
    private static readonly ConcurrentDictionary<TimeSpan, CookieOptions> ExpirationCookieOptions = new();

    public static void AppendSessionCookie(this IResponseCookies cookies, Session session, ILogger? log = null)
    {
        var sid = session.Id.ToString();
        log?.Verbose("Appending session cookie for session id {sessionId}", sid);
        cookies.Append(SessionAuthenticationFilter.SessionIdCookie, sid, ExpirationCookieOptions.GetOrAdd(session.Expiration, CookieOptionGen));
    }

    private readonly static Func<TimeSpan, CookieOptions> CookieOptionGen = ts => new CookieOptions()
    {
        Expires = DateTimeOffset.Now + ts,
        SameSite = SameSiteMode.Strict,
        Secure = true
    };

    public static bool CheckIfAlreadyLoggedIn(User user, ILogger? log = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        log?.Debug("Checking if user is already logged in");
        if (user.PasswordHash is not null)
        {
            log?.Information("User {user} ({userid}) is already logged in");
            return true;
        }
        return false;
    }

    public static bool RedirectIfLoggedIn(User user, HttpContext context, string redirectUri = "/index", ILogger? log = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(redirectUri);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(context);

        var r = CheckIfAlreadyLoggedIn(user, log);
        if (r)
        {
            log?.Information("Redirecting to {uri}", redirectUri);
            context.Response.Redirect(redirectUri);
            return true;
        }
        return false;
    }
}
