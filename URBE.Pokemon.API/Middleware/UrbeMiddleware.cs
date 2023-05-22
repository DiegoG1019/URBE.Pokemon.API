using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Middleware;

public abstract class UrbeMiddleware
{
    protected virtual ILogger CreateLogger(HttpContext context)
        => LogHelper.CreateLogger(
            "Middleware",
            $"Middleware: {GetType().Name}",
            null,
            new LogProperty("Trace", context.TraceIdentifier)
        );
}
