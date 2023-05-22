namespace URBE.Pokemon.API.Models.Database;

public interface IDispatchable
{
    public DateTimeOffset? ClaimedAt { get; set; }
    public Server? ClaimedBy { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
}
