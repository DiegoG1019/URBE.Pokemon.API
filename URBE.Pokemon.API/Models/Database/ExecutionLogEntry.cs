using Serilog.Events;

namespace URBE.Pokemon.API.Models.Database;

public class ExecutionLogEntry 
{
    public long Id { get; init; }
    public required DateTimeOffset Date { get; init; }
    public required string Message { get; init; }
    public required string ClientName { get; init; }
    public required LogEventLevel LogEventLevel { get; init; }
    public string? TraceId { get; init; }
    public string? LoggerName { get; init; }
    public string? ExceptionType { get; init; }
    public string? ExceptionMessage { get; init; }
    public string? ExceptionDumpPath { get; init; }
    public string? Area { get; init; }
    public string? JsonProperties { get; init; }

    public string? Username { get; init; }
    public Id<User>? UserId { get; init; }
    public Id<Session>? SessionId { get; init; }
}
