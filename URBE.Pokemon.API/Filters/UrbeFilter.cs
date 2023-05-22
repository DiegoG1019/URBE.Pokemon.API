using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Filters;

public abstract class UrbeFilter
{
    protected virtual ILogger CreateLogger(HttpContext context)
        => LogHelper.CreateLogger(
            "Filter",
            $"Filter: {GetType().Name}",
            null,
            new LogProperty("Trace", context.TraceIdentifier)
        );
}
