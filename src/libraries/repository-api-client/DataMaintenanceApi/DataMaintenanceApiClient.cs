using Microsoft.Extensions.Options;
using RestSharp;

namespace XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;

public class DataMaintenanceApiClient : BaseApiClient, IDataMaintenanceApiClient
{
    public DataMaintenanceApiClient(IOptions<RepositoryApiClientOptions> options) : base(options)
    {
    }

    public async Task PruneChatMessages(string accessToken)
    {
        await ExecuteAsync(CreateRequest("repository/data-maintenance/prune-chat-messages", Method.Delete,
            accessToken));
    }

    public async Task PruneGameServerEvents(string accessToken)
    {
        await ExecuteAsync(
            CreateRequest("repository/data-maintenance/prune-game-server-events", Method.Delete, accessToken));
    }
}