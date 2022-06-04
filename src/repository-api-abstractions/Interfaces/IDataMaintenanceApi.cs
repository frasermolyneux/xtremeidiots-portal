namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

public interface IDataMaintenanceApi
{
    Task PruneChatMessages();
    Task PruneGameServerEvents();
    Task PruneGameServerStats();
    Task PruneRecentPlayers();
}