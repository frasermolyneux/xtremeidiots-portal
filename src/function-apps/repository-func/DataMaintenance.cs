using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc;

public class DataMaintenance
{
    private readonly ILogger<DataMaintenance> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;
    private readonly IRepositoryTokenProvider _repositoryTokenProvider;

    public DataMaintenance(
        ILogger<DataMaintenance> log,
        IRepositoryTokenProvider repositoryTokenProvider,
        IRepositoryApiClient repositoryApiClient)
    {
        _log = log;
        _repositoryTokenProvider = repositoryTokenProvider;
        _repositoryApiClient = repositoryApiClient;
    }

    [FunctionName("DataMaintenance")]
    public async Task RunDataMaintenance([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
    {
        _log.LogInformation("Performing Data Maintenance");

        var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();
        await _repositoryApiClient.DataMaintenance.PruneChatMessages(accessToken);
        await _repositoryApiClient.DataMaintenance.PruneGameServerEvents(accessToken);
    }
}