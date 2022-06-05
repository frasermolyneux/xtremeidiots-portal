using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api;

public class DataMaintenanceApi : BaseApi, IDataMaintenanceApi
{
    public DataMaintenanceApi(ILogger<DataMaintenanceApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
    {

    }

    public async Task PruneChatMessages()
    {
        await ExecuteAsync(await CreateRequest("data-maintenance/prune-chat-messages", Method.Delete));
    }

    public async Task PruneGameServerEvents()
    {
        await ExecuteAsync(await CreateRequest("data-maintenance/prune-game-server-events", Method.Delete));
    }

    public async Task PruneGameServerStats()
    {
        await ExecuteAsync(await CreateRequest("data-maintenance/prune-game-server-stats", Method.Delete));
    }

    public async Task PruneRecentPlayers()
    {
        await ExecuteAsync(await CreateRequest("data-maintenance/prune-recent-players", Method.Delete));
    }
}