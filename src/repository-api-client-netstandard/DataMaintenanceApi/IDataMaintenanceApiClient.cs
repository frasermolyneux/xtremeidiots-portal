using System.Threading.Tasks;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DataMaintenanceApi
{
    public interface IDataMaintenanceApiClient
    {
        Task PruneChatMessages(string accessToken);
        Task PruneGameServerEvents(string accessToken);
    }
}