namespace URBE.Pokemon.API.Models.Database;

public class Session : IKeyed<Session>
{
    public required Id<Session> Id { get; init; }
    public required Id<User> UserId { get; init; }
    public User User { get; init; }
    public required TimeSpan Expiration { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required DateTimeOffset LastUsed { get; set; }
}