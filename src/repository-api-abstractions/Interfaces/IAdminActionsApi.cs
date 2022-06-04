using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IAdminActionsApi
    {
        Task<AdminActionDto?> GetAdminAction(Guid adminActionId);
        Task<List<AdminActionDto>?> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filter, int skipEntries, int takeEntries, AdminActionOrder? order);
        Task DeleteAdminAction(Guid adminActionId);
    }
}