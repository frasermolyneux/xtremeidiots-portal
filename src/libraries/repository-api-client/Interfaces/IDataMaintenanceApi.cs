namespace XtremeIdiots.Portal.RepositoryApiClient.Interfaces;

public interface IDataMaintenanceApi
{
    Task PruneChatMessages();
    Task PruneGameServerEvents();
    Task PruneGameServerStats();
    Task PruneRecentPlayers();
}