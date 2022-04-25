namespace XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;

public interface IDataMaintenanceApiClient
{
    Task PruneChatMessages(string accessToken);
    Task PruneGameServerEvents(string accessToken);
}