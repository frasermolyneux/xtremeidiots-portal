using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Threading.Tasks;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DataMaintenanceApi
{
    public class DataMaintenanceApiClient : BaseApiClient, IDataMaintenanceApiClient
    {
        public DataMaintenanceApiClient(ILogger<DataMaintenanceApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {

        }

        public async Task PruneChatMessages(string accessToken)
        {
            await ExecuteAsync(CreateRequest("repository/DataMaintenance/PruneChatMessages", Method.DELETE, accessToken));
        }

        public async Task PruneGameServerEvents(string accessToken)
        {
            await ExecuteAsync(CreateRequest("repository/DataMaintenance/PruneGameServerEvents", Method.DELETE, accessToken));
        }
    }
}