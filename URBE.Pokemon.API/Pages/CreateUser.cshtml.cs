using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Models.Requests;
using URBE.Pokemon.API.Pages.Shared;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Pages;

[AuthenticateSession]
public class CreateUserModel : UrbeAuthenticatedPage
{
    private readonly UserManager Users;
    private readonly UrbeContext Db;
    private readonly SessionManager Sessions;

    [BindProperty]
    public NewUserRequest? NewUserRequest { get; set; }

    public CreateUserModel(UserManager users, SessionManager sessions, UrbeContext db)
    {
        Users = users ?? throw new ArgumentNullException(nameof(users));
        Db = db ?? throw new ArgumentNullException(nameof(db));
        Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Log.Debug("Processing new create user request");

        if (HttpHelpers.CheckIfAlreadyLoggedIn(UrbeUser, Log))
        {
            Log.Information("Redirecting to /index");
            return Redirect("/index");
        }

        if (ModelState.IsValid is false || NewUserRequest is null)
        {
            AddError("The request body is invalid or is null");
            Log.Debug("Bad request for new create user");
            return Page(HttpStatusCode.BadRequest);
        }

        Log.Verbose("Verifying if create user request for {username} of {Email} is valid", NewUserRequest.Username, NewUserRequest.Email);

        if (await Users.CheckIfUsernameExists(NewUserRequest.Username))
        {
            Log.Verbose("The username {username} is already claimed by another user", NewUserRequest.Username);
            AddError("The username already exists");
        }

        if (MailAddress.TryCreate(NewUserRequest.Email, out var mail) is false)
        {
            Log.Verbose("The email {mail} is not a valid email", NewUserRequest.Email);
            AddError("The email is not valid");
        }
        else if (await Users.CheckIfEmailExists(mail))
        {
            Log.Verbose("The email {mail} already belongs to another user", NewUserRequest.Email);
            AddError("This email already belongs to another user");
        }

        if (HasErrors)
        {
            Log.Debug("The request was not validated properly");
            return Page(HttpStatusCode.Conflict);
        }

        Log.Verbose("Creating new user object");

        var uu = await Db.Users.FindAsync(UrbeUser.Id);

        if (uu is null)
        {
            uu = new User()
            {
                Id = Id<User>.New(),
                Username = ""
            };
            Db.Users.Add(uu);
        }

        uu.PasswordHash = Helper.GetHash512(NewUserRequest.Password);
        uu.Username = NewUserRequest.Username;
        uu.EmailAddress = mail;
        await Db.SaveChangesAsync();

        Log.Debug("Adding new user to database");

        Log.Debug("Creating new session for user {user} ({id})", uu.DisplayName, uu.Id);
        var session = await Sessions.CreateSession(uu);

        Response.Cookies.AppendSessionCookie(session, Log);

        Log.Information("Succesfully processed a new create user request for user {user} ({id})", uu.DisplayName, uu.Id);
        return Redirect("/Index");
    }

    public void OnGet()
    {
        Log.Information("Getting create user page");
        HttpHelpers.RedirectIfLoggedIn(UrbeUser, HttpContext, log: Log);
    }
}
