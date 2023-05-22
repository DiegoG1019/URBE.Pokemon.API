using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Pages.Shared;

public class UrbePage : PageModel
{
    private ILogger? _log;
    protected ILogger Log => _log ??= CreateLogger();

    private List<string>? _err;
    private List<string>? _msg;
    protected void AddError(string error)
        => (_err ??= new()).Add(error);

    public bool HasErrors
        => _err?.Count is > 0;

    public IEnumerable<string> Errors
        => _err ?? Enumerable.Empty<string>();
     
    protected void AddMessage(string message)
        => (_msg ??= new()).Add(message);

    public bool HasMessages
        => _msg?.Count is > 0;

    public IEnumerable<string> Messages
        => _err ?? Enumerable.Empty<string>();

    public virtual IActionResult Page(int httpStatusCode)
    {
        var p = Page();
        p.StatusCode = httpStatusCode;
        return p;
    }

    public virtual IActionResult Page(HttpStatusCode statusCode)
        => Page((int)statusCode);

    protected virtual ILogger CreateLogger()
        => LogHelper.CreateLogger(
            "Pages",
            GetType().Name,
            null,
            new LogProperty("PageName", GetType().Name),
            new LogProperty("Trace", HttpContext.TraceIdentifier)
        );
}
