using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Interfaces
{
    public interface IAdminActionsApi
    {
        Task<AdminActionDto?> GetAdminAction(Guid adminActionId);
        Task<List<AdminActionDto>?> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filterType, int skipEntries, int takeEntries, AdminActionOrder? order);
        Task DeleteAdminAction(Guid adminActionId);
    }
}