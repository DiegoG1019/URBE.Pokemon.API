using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Workers;

[RegisterUrbeWorker]
public class HeartbeatService : ApiServiceWorker
{
    public HeartbeatService(IServiceProvider rootProvider) : base(rootProvider)
    {
    }

    public override async Task Work(CancellationToken stoppingToken)
    {
        using var services = GetNewScopedServices();
        using var db = services.GetRequiredService<UrbeContext>();

        Log.Verbose("Sending heartbeat signal");

        var server = Program.Server;
        var serverid = server.Id;
        var dtnow = DateTimeOffset.Now;
        var interval = Program.Settings.HeartbeatInterval;

        server.LastHeartbeat = dtnow;
        server.HeartbeatInterval = interval;
        if (await db.Servers.AnyAsync(x => x.Id == serverid, stoppingToken))
            await db.Servers
                .Where(x => x.Id == serverid)
                .ExecuteUpdateAsync(x => x.SetProperty(x => x.LastHeartbeat, dtnow).SetProperty(x => x.HeartbeatInterval, interval), stoppingToken);
        else
        {
            Log.Information("Registering Server \"{name}\" ({id}) in database", server.Name, server.Id);
            db.Servers.Add(server);
            await db.SaveChangesAsync(stoppingToken);

            Log.Debug("Succesfully registered Server \"{name}\" ({id}) in database", server.Name, server.Id);
        }

        await Task.Delay(interval, stoppingToken);
    }
}
