using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Pages.Shared;

namespace URBE.Pokemon.API.Pages;

[AuthenticateSession]
public class PrivacyModel : UrbeAuthenticatedPage
{
    public void OnGet()
    {
        Log.Debug("Getting Privacy Page");
    }
}

