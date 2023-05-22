namespace URBE.Pokemon.API.Models.Database;

public class Server : IKeyed<Server>
{
    public required Id<Server> Id { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset Registered { get; init; }
    public DateTimeOffset LastHeartbeat { get; set; }
    public TimeSpan HeartbeatInterval { get; set; }
}
