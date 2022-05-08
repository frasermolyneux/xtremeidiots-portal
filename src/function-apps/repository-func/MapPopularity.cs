using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc;

public class MapPopularity
{
    private readonly ILogger<MapPopularity> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;
    private readonly IRepositoryTokenProvider _repositoryTokenProvider;

    public MapPopularity(
        ILogger<MapPopularity> log,
        IRepositoryTokenProvider repositoryTokenProvider,
        IRepositoryApiClient repositoryApiClient)
    {
        _log = log;
        _repositoryTokenProvider = repositoryTokenProvider;
        _repositoryApiClient = repositoryApiClient;
    }

    [FunctionName("RebuildMapPopularity")]
    public async Task RunRebuildMapPopularity([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer)
    {
        _log.LogInformation("Performing Rebuild of Map Popularity");

        var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();
        await _repositoryApiClient.Maps.RebuildMapPopularity(accessToken);
    }
}