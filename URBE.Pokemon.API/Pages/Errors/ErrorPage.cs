using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using URBE.Pokemon.API.Pages.Shared;

namespace URBE.Pokemon.API.Pages.Errors;

public class ErrorPage : UrbePage
{
    [FromQuery]
    public int? Status { get; set; }

    [FromQuery]
    public string? TraceId { get; set; }

    public const bool IsDebug =
#if DEBUG
        true;
#else
        false;
#endif

    public string? RequestId => TraceId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet()
    {
        HttpContext.Response.StatusCode = Status ?? HttpContext.Response.StatusCode;
    }
}
