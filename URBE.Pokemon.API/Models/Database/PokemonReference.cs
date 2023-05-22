namespace URBE.Pokemon.API.Models.Database;

public class PokemonReference
{
    public long Id { get; init; }
    public required int PokemonId { get; init; }
    public required Id<User>? UserId { get; init; }
    public required Id<PokemonList>? ListId { get; init; }

    public User? User { get; init; }
    public PokemonList? List { get; init; }

    public required DateTimeOffset CreationDate { get; init; }
}
