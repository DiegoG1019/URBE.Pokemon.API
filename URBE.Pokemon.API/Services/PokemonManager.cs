using URBE.Pokemon.API.Models.Database;

namespace URBE.Pokemon.API.Services;

public class PokemonManager
{
    private readonly UrbeContext Db;

    public PokemonManager(UrbeContext context)
    {
        Db = context;
    }

    public Task AddToSearchHistory(int pokeId, User user)
    {
        if (pokeId <= 0) throw new ArgumentOutOfRangeException(nameof(pokeId), "pokeId must be a valid National Dex Id");
        ArgumentNullException.ThrowIfNull(user);

        user.VisitHistory.Add(new PokemonReference()
        {
            CreationDate = DateTimeOffset.Now,
            ListId = null,
            UserId = user.Id,
            PokemonId = pokeId
        });

        return Db.SaveChangesAsync();
    }

    public Task AddToList(int pokeId, PokemonList list)
    {
        if (pokeId <= 0) throw new ArgumentOutOfRangeException(nameof(pokeId), "pokeId must be a valid National Dex Id");
        ArgumentNullException.ThrowIfNull(list);

        list.Pokemon.Add(new PokemonReference()
        {
            CreationDate = DateTimeOffset.Now,
            ListId = list.Id,
            UserId = null,
            PokemonId = pokeId
        });

        return Db.SaveChangesAsync();
    }
}
