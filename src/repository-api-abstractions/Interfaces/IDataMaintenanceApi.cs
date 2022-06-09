using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

public interface IDataMaintenanceApi
{
    Task<ApiResponseDto> PruneChatMessages();
    Task<ApiResponseDto> PruneGameServerEvents();
    Task<ApiResponseDto> PruneGameServerStats();
    Task<ApiResponseDto> PruneRecentPlayers();
}