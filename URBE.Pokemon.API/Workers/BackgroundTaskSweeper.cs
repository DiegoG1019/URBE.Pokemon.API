using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Workers;

[RegisterUrbeWorker]
public class BackgroundTaskSweeper : ApiWorker
{
    public override async Task Work(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);
        await BackgroundTaskStore.Sweep();
    }
}
