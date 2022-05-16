using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc;

public class MapPopularity
{
    private readonly ILogger<MapPopularity> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;

    public MapPopularity(
        ILogger<MapPopularity> log,
        IRepositoryApiClient repositoryApiClient)
    {
        _log = log;
        _repositoryApiClient = repositoryApiClient;
    }

    [FunctionName("RebuildMapPopularity")]
    public async Task RunRebuildMapPopularity([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer)
    {
        _log.LogInformation("Performing Rebuild of Map Popularity");

        await _repositoryApiClient.Maps.RebuildMapPopularity();
    }
}