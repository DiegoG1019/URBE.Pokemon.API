using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Pages.Shared;

namespace URBE.Pokemon.API.Pages;

[AuthenticateSession]
public class IndexModel : UrbeAuthenticatedPage
{
    [FromQuery]
    public int? PokePage { get; set; }

    public int PageIndex => PokePage is null or < 0 ? 0 : PokePage.Value;

    public void OnGet()
    {
        
    }
}
