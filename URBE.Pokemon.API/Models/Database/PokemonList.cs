namespace URBE.Pokemon.API.Models.Database;

public class PokemonList : IKeyed<PokemonList>
{
    public required Id<PokemonList> Id { get; init; }
    public required Id<User> UserId { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public HashSet<PokemonReference> Pokemon { get; } = new();
    public User User { get; init; }
}
