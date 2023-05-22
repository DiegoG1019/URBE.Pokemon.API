using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Pages.Shared;

public class LayoutModel : PageModel
{
    public User? UrbeUser => HttpContext.Features.Get<User>();
}
