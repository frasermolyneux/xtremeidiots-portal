namespace XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;

public interface IDataMaintenanceApiClient
{
    Task PruneChatMessages();
    Task PruneGameServerEvents();
    Task PruneGameServerStats();
    Task PruneRecentPlayers();
}