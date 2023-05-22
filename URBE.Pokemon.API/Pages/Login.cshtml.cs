using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Models.Requests;
using URBE.Pokemon.API.Pages.Shared;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Pages;

[AuthenticateSession]
public class LoginModel : UrbeAuthenticatedPage
{
    private readonly UserManager Users;
    private readonly SessionManager Sessions;

    [BindProperty]
    public LoginRequest? LoginRequest { get; set; }

    public LoginModel(UserManager users, SessionManager sessions)
    {
        Users = users ?? throw new ArgumentNullException(nameof(users));
        Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Log.Debug("Processing new login request");
        if (HttpHelpers.CheckIfAlreadyLoggedIn(UrbeUser, Log))
        {
            Log.Information("Redirecting to /index");
            return Redirect("/index");
        }

        if (ModelState.IsValid is false || LoginRequest is null) 
        {
            AddError("The request body is invalid or is null");
            Log.Debug("Bad request for new login");
            return Page(HttpStatusCode.BadRequest);
        }

        Log.Verbose("Verifying if login request for {username} is valid", LoginRequest.Username);
        var user = await Users.CheckLogin(LoginRequest);

        if (user is null)
        {
            AddError("Could not find an user with the given credentials");
            Log.Debug("Could not verify user credentials for {username}", LoginRequest.Username);
            return Page(HttpStatusCode.Forbidden);
        }

        Debug.Assert(user is not null);

        Log.Debug("Creating new session for user {user} ({id})", user.DisplayName, user.Id);
        var session = await Sessions.CreateSession(user);
        Response.Cookies.AppendSessionCookie(session, Log);

        Log.Information("Succesfully processed a login request for user {user} ({id})", user.DisplayName, user.Id);
        return Redirect("/Index");
    }

    public void OnGet()
    {
        Log.Information("Getting Login Page");
        HttpHelpers.RedirectIfLoggedIn(UrbeUser, HttpContext, log: Log);
    }
}
