using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;

public class DataMaintenanceApiClient : BaseApiClient, IDataMaintenanceApiClient
{
    public DataMaintenanceApiClient(ILogger<DataMaintenanceApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
    {

    }

    public async Task PruneChatMessages()
    {
        await ExecuteAsync(await CreateRequest("repository/data-maintenance/prune-chat-messages", Method.Delete));
    }

    public async Task PruneGameServerEvents()
    {
        await ExecuteAsync(await CreateRequest("repository/data-maintenance/prune-game-server-events", Method.Delete));
    }
}