using System.Runtime.CompilerServices;

namespace URBE.Pokemon.API;

// Use the accept-language header to set the language

public static class DebugFlags
{
    private static readonly string[] Args = Environment.GetCommandLineArgs();
    private static bool CheckArgs([CallerMemberName] string? callerName = null)
        => Args.Contains(callerName);

    public static bool ClearDatabase => CheckArgs();
}
