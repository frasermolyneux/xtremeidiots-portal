using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc;

public class DataMaintenance
{
    private readonly ILogger<DataMaintenance> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;

    public DataMaintenance(
        ILogger<DataMaintenance> log,

        IRepositoryApiClient repositoryApiClient)
    {
        _log = log;
        _repositoryApiClient = repositoryApiClient;
    }

    [FunctionName("DataMaintenance")]
    public async Task RunDataMaintenance([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
    {
        _log.LogInformation("Performing Data Maintenance");

        await _repositoryApiClient.DataMaintenance.PruneChatMessages();
        await _repositoryApiClient.DataMaintenance.PruneGameServerEvents();
    }
}