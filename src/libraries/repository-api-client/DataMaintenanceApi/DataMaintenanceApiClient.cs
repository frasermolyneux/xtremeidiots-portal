using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;

public class DataMaintenanceApiClient : BaseApiClient, IDataMaintenanceApiClient
{
    public DataMaintenanceApiClient(ILogger<DataMaintenanceApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
    {

    }

    public async Task PruneChatMessages(string accessToken)
    {
        await ExecuteAsync(CreateRequest("repository/DataMaintenance/PruneChatMessages", Method.Delete, accessToken));
    }

    public async Task PruneGameServerEvents(string accessToken)
    {
        await ExecuteAsync(CreateRequest("repository/DataMaintenance/PruneGameServerEvents", Method.Delete, accessToken));
    }
}