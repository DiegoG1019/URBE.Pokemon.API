using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Pages.Shared;

namespace URBE.Pokemon.API.Pages;
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : UrbePage
{
    public const bool IsDebug =
#if DEBUG
        true;
#else
        false;
#endif

    public string? RequestId => Activity.Current?.Id ?? HttpContext.TraceIdentifier;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

