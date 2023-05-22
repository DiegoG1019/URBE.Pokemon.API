using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using URBE.Pokemon.API.Attributes;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class AuthorizeUrbeUserAttribute : TypeFilterAttribute
{
    public AuthorizeUrbeUserAttribute(UserPermissions userPermissions = 0) : base(typeof(UrbeUserAuthorizationFilter))
    {
        Arguments = new object[] { userPermissions };
    }
}

//[RegisterUrbeService(ServiceLifetime.Scoped)]
public class UrbeUserAuthorizationFilter : UrbeFilter, IAsyncAuthorizationFilter
{
    private readonly UserPermissions UserPermissions;
    private readonly SessionAuthenticationFilter TokenAuthFilter;

    public UrbeUserAuthorizationFilter(UserPermissions userPermissions, SessionManager sessions, UserManager users)
    {
        TokenAuthFilter = new SessionAuthenticationFilter(sessions, users);
        UserPermissions = userPermissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        await TokenAuthFilter.OnAuthorizationAsync(context);
        if (context.Result is not null) return;

        var log = CreateLogger(context.HttpContext);
        var user = context.HttpContext.Features.Get<User>();
        Debug.Assert(user is not null); // Este valor no puede ser nulo si context.Result no lo es; para llegar a este punto el filtro de autenticacion tuvo que haber logrado algo
                                        // Es decir, tuvo que haber encontrado el usuario y por tanto tener un token, o context.Result NO es nulo y no deberia llegar hasta aca

        if (UserPermissions > 0)
        {
            log.Debug("Attempting to authorize user {user} ({userid}) for user permissions: {permissions}", user.DisplayName, user.Id, UserPermissions);
            UserPermissions p = user.UserPermissions;

            if (p.HasFlag(UserPermissions) is false)
            {
                log.Debug("The user {user} ({userid}) did not have the required user permissions of {permissions} and was not authorized", user.DisplayName, user.Id, UserPermissions);
                context.Result = new RedirectResult($"errors/unauthorized?status=403&trace={context.HttpContext.TraceIdentifier}");
                return;
            }
        }

        log.Debug("The user {user} ({userid}) was succesfully authorized for permissions {permissions}", user.DisplayName, user.Id, UserPermissions);
    }
}
