using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.AdminActionsApi
{
    public interface IAdminActionsApiClient
    {
        Task<AdminActionDto?> GetAdminAction(Guid adminActionId);
        Task<List<AdminActionDto>?> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filterType, int skipEntries, int takeEntries, AdminActionOrder? order);
        Task DeleteAdminAction(Guid adminActionId);
    }
}