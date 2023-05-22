using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokeApiNet;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Pages.Shared;

namespace URBE.Pokemon.API.Pages;

[AuthenticateSession]
public class PokemonModel : UrbeAuthenticatedPage
{
    public int? PokemonId { get; set; }

    public void OnGet(int? pokemonId)
    {
        Log.Debug("Getting Pokemon Data Page");

        if (pokemonId is null or < 0)
        {
            Log.Debug("Requested pokemon page with null or invalid id {id}", pokemonId);
            if (pokemonId is not null)
                AddError($"No se encontró un Pokemon bajo el número de PokeDex nacional {pokemonId}");
            else
                AddError($"Por favor provea un número de PokeDex nacional");
        }
        else
            PokemonId = pokemonId;
    }
}
