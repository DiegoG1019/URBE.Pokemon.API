using Microsoft.AspNetCore.Mvc;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Pages.Shared;

public class UrbeAuthenticatedPage : UrbePage
{
    public Guid UserId { get; init; }

    private User? _user;
    public User UrbeUser => _user ??= HttpContext.Features.Get<User>() ?? throw new InvalidDataException("There is no user available for this page's request");

    private Session? _session;
    public Session Session => _session ??= HttpContext.Features.Get<Session>() ?? throw new InvalidDataException("There is no session available for this page's request");

    protected override ILogger CreateLogger()
        => LogHelper.CreateLogger(
            "Pages",
            GetType().Name,
            null,
            new LogProperty("PageName", GetType().Name),
            new LogProperty("Username", UrbeUser?.Username),
            new LogProperty("UserId", UrbeUser?.Id),
            new LogProperty("SessionId", Session?.Id)
        );
}
